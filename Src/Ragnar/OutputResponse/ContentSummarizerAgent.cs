namespace Ragnar.OutputResponse;

/// <summary>
/// AI agent that summarizes directory contents and answers questions via Ollama.
/// </summary>
/// <example><![CDATA[[var sum = await agent.SummarizeContent(path, q, ct);]]></example>
public class ContentSummarizerAgent(
    IOllamaChatClientBuilder ollamaAIClientBuilder,
    IOllamaGenerationService ollamaClientProvider,
    IOptions<RagnarConfig> options,
    IOllamaClientFactory factory,
    [FromKeyedServices("Summary")] IChatPromptProvider summaryPrompt)
{
    /// <summary>Generates a high-level summary of files in a directory.</summary>
    /// <param name ="folder"> Directory path to analyze and summarize.</param>
    /// <param name ="question"> Inquiry context for the AI agent.</param>
    /// <param name ="cancellationToken"> Cancellation Token to abort processing.</param>
    /// <returns>A concise textual summary of key insights found.</returns>
    /// <example><![CDATA[var sum = await agent.SummarizeContent(path, q, ct);]]></example>
    [Description("Summarize files of provided folder contents.")]
    public async Task<string> SummarizeContent(
    [Description("The directory path to loop over.")] string folder,
    [Description("The question to be asked.")] string question,
    CancellationToken cancellationToken)
    {
        var contents = await LoadFolderContentsAsync(folder, cancellationToken)
            .ConfigureAwait(false);

        var request = new GenerateRequest
        {
            Prompt = summaryPrompt.GetTemplate(contents, question),
            System = $"{summaryPrompt.System}"
        };

        var policy = Policy.Handle<HttpRequestException>()
            .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)));

        return await policy.ExecuteAsync(async () => await ollamaClientProvider.GenerateResponse(request, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);
    }

    /// <summary>Gets the configured AI agent with tools and instructions.</summary>
    private AIAgent AiAgent => SetupAgent(ollamaAIClientBuilder);

    /// <summary>Builds and returns a configured <see cref="ChatClientAgent"/> instance.</summary>
    /// <param name="ollamaAIClientBuilder">The client builder for Ollama connections.</param>
    /// <returns>A ready-to-use AI agent with summarization tools.</returns>
    /// <example><![CDATA[var agent = SetupAgent(builder);]]></example>
    private ChatClientAgent SetupAgent(IOllamaChatClientBuilder ollamaAIClientBuilder)
    {
        var a = options.Value.OllamaOptions;
        var b = options.Value.GenerationOptions;

        IChatClient agent = new OllamaChatClientBuilder(factory)
            .WithChatClient(OllamaServiceType.Ollama)
            .WithModel(a.LlmModel)
            .WithTemperature(b.Temperature)
            .WithMaxTokens(b.MaxTokens)
            .WithFunctionInvocation()
            .WithMiddleware()
            .Build();

        //x.WithRetryPolicy(3, backoff: TimeSpan.FromSeconds(2)),
        //x.WithLogging()

        return agent.AsAIAgent(
            instructions: summaryPrompt.GetTemplate("", ""),
            name: "SummarizeAgent",
            tools: [AIFunctionFactory.Create(SummarizeContent)]);
    }

    /// <summary>Executes an interactive AI query against directory files.</summary>
    /// <param name = "folder"> Directory path containing target files.</param>
    /// <param name = "question"> Specific inquiry for the agent to answer.</param>
    /// <param name = "cancellationToken"> Cancellation Token to abort processing.</param>
    /// <returns>AI-generated response text addressing the question.</returns>
    /// <example><![CDATA[var resp = await agent.AskAgent(path, q, ct);]]></example>
    public async Task<string> AskAgent(
        string folder,
        string question,
        CancellationToken cancellationToken)
    {
        var response = await AiAgent
            .RunAsync($"Read all the files in the directory {folder} and ask the follow question, {question}", cancellationToken: cancellationToken).ConfigureAwait(false);

        return response.Text;
    }

    ///<summary>Reads and concatenates all files in a target directory.</summary>
    ///<param name = "folder"> Directory path to read files from.</param>
    ///<param name = "cancellationToken"> Cancellation Token to abort reading.</param>
    ///<returns>A combined string of all file contents.</returns>
    ///<example><![CDATA[var txt = await agent.LoadFolderContentsAsync(path, ct);]]></example>
    private static async Task<string> LoadFolderContentsAsync(string folder, CancellationToken cancellationToken)
    {
        var files = Directory.EnumerateFiles(folder).ToList();

        if (files.Count == 0)
            return "No items to summarize. Please ignore.";

        var builder = new StringBuilder();

        foreach (var file in files)
        {
            using var reader = File.OpenText(file);
            var text = await reader.ReadToEndAsync(cancellationToken)
                .ConfigureAwait(false);

            builder.AppendLine($"---\n{AppDefaults.FileMarkerStart}{Path.GetFileName(file)}{AppDefaults.FileMarkerEnd}\n{text}\n");
        }

        return $"{AppDefaults.CodeBlockEnd} {builder} {AppDefaults.CodeBlockEnd}";
    }
}
