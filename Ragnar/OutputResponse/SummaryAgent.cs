namespace Ragnar.OutputResponse;

/// <summary>Initializes a new instance of the summary agent orchestrator.</summary>
/// <param name ="ClientFactory"> Ollama client factory.</param>
/// <param name ="OllamaClientProvider"> Client provider used for LLM response generation.</param>
/// <param name ="SummaryPrompt"> System prompt template used for content summaries.</param>
public class SummaryAgent(
    IOllamaClientFactory ClientFactory,
    IOllamaResponse OllamaClientProvider,
    [FromKeyedServices("Summary")] ISystemPromptProvider SummaryPrompt)
{
    private const string PROMPT = "Based on the following code-related QuestionRequest&A responses, produce a concise, high-level summary of key insights, patterns, recommendations and priorities. Keep it under 1000 words.";

    /// <summary>
    /// Holds the underlying chat client instance for LLM communication.
    /// </summary>
    private readonly IChatClient ChatClient =
        ClientFactory.FindClient(OllamaServiceType.Ollama);

    /// <summary>Generates a high-level summary of files in a directory.</summary>
    /// <param name ="Folder"> Directory path to analyze and summarize.</param>
    /// <param name ="Question"> Inquiry context for the AI agent.</param>
    /// <param name ="Ct"> Cancellation Token to abort processing.</param>
    /// <returns>A concise textual summary of key insights found.</returns>
    /// <example><![CDATA[var sum = await agent.SummarizeContent(path, q, ct);]]></example>
    [Description("Summarize files of provided folder contents.")]
    public async Task<string> SummarizeContent(
    [Description("The directory path to loop over.")] string Folder,
    [Description("The question to be asked.")] string Question,
    CancellationToken Ct)
    {
        var contents = await LoadFolderContentsAsync(Folder, Ct);

        SummaryPrompt.Content = contents;

        var request = new GenerateRequest
        {
            Prompt = SummaryPrompt.Template,
            System = SummaryPrompt.Template + Question
        };

        return await OllamaClientProvider.GenerateResponse(request, Ct);
    }

    /// <summary>Executes an interactive AI query against directory files.</summary>
    /// <param name = "Folder"> Directory path containing target files.</param>
    /// <param name = "Question"> Specific inquiry for the agent to answer.</param>
    /// <param name = "Ct"> Cancellation Token to abort processing.</param>
    /// <returns>AI-generated response text addressing the question.</returns>
    /// <example><![CDATA[var resp = await agent.AskAgent(path, q, ct);]]></example>
    public async Task<string> AskAgent(string Folder, string Question, CancellationToken Ct)
    {
        var agent = ChatClient.AsBuilder()
            .UseFunctionInvocation()
            .Build()
            .AsAIAgent(
                instructions: PROMPT,
                name: "SummarizeAgent",
                tools: [AIFunctionFactory.Create(SummarizeContent)]);

        var response = await agent.RunAsync($"Read all the files in the directory {Folder} and ask the follow question, {Question}", cancellationToken: Ct);

        return response.Text;
    }

    ///<summary>Reads and concatenates all files in a target directory.</summary>
    ///<param name = "Folder"> Directory path to read files from.</param>
    ///<param name = "Ct"> Cancellation Token to abort reading.</param>
    ///<returns>A combined string of all file contents.</returns>
    private static async Task<string> LoadFolderContentsAsync(string Folder, CancellationToken Ct)
    {
        var files = Directory.GetFiles(Folder);
        if (files.Length == 0)
        {
            return "No items to summary. Please ignore.";
        }

        // Pre-allocate array to avoid dynamic resizing
        var pooledBuffer = ArrayPool<char>.Shared.Rent(8192);

        var contents = new List<string>();

        await Parallel.ForEachAsync(files, new ParallelOptions { CancellationToken = Ct }, async (file, token) =>
        {
            using var reader = File.OpenText(file);
            int bytesRead;
            var text = new StringBuilder();
            while ((bytesRead = await reader.ReadAsync(pooledBuffer.AsMemory(0, pooledBuffer.Length), token)) > 0)
            {
                text.Append(pooledBuffer.AsSpan(0, bytesRead));
            }

            contents.Add($"---\n[RESPONSE_FILE]{Path.GetFileName(file)}[/RESPONSE_FILE]\n{text}\n");
        });

        // string.Join is highly optimized and avoids StringBuilder thread-safety issues
        return "[RESPONSE_CODE] " + string.Concat(contents) + "[/RESPONSE_CODE]";
    }
}
