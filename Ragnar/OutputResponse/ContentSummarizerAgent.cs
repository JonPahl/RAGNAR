namespace Ragnar.OutputResponse;

/// <summary>Initializes a new instance of the SummaryAgent orchestrator.</summary>
/// <param name ="ollamaClientProvider"> Client provider used for LLM response generation.</param>
/// <param name ="summaryPrompt"> System prompt template used for content summaries.</param>
public class ContentSummarizerAgent(
    IOllamaAIClientBuilder ollamaAIClientBuilder,
    IOllamaGenerationService ollamaClientProvider,
    [FromKeyedServices("Summary")] IPromptProvider summaryPrompt)
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
        var contents = await LoadFolderContentsAsync(folder, cancellationToken).ConfigureAwait(false);

        var request = new GenerateRequest
        {
            Prompt = summaryPrompt.GetTemplate(contents, question),
            System = $"{summaryPrompt.System}\n{question}"
        };

        var policy = Policy.Handle<HttpRequestException>()
            .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)));

        return await policy.ExecuteAsync(async () => await ollamaClientProvider.GenerateResponse(request, cancellationToken).ConfigureAwait(false)).ConfigureAwait(false);
    }

    private AIAgent AiAgent => SetupAgent(ollamaAIClientBuilder);

    private ChatClientAgent SetupAgent(IOllamaAIClientBuilder ollamaAIClientBuilder)
    {
        var agent = ollamaAIClientBuilder
        .WithChatClient(OllamaServiceType.Ollama)
        .Build();

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
    public async Task<string> AskAgent(string folder, string question, CancellationToken cancellationToken)
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
            var text = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

            builder.AppendLine($"---\n{AppDefaults.FileMarkerStart}{Path.GetFileName(file)}{AppDefaults.FileMarkerEnd}\n{text}\n");
        }

        return $"{AppDefaults.CODE_BLOCK_START} {builder} {AppDefaults.CODE_BLOCK_END}";
    }
}
