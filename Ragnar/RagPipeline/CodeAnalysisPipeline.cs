namespace Ragnar.RagPipeline;

/// <summary>Executes RAG pipeline: embeds question → retrieve → generate answer.</summary>
/// <param name="writer">Custom console writer.</param>
/// <param name="ConfigWrapper">Wraps all options.</param>
/// <param name="systemPromptProvider">System prompt provider.</param>
/// <param name="SaveService">Response writer service.</param>
/// <param name="ollamaClientFactory">OllamaOptions client factory.</param>
/// <param name="ollamaProvider">Ollama question calling operations.</param>
/// <example><![CDATA[await new RagPipeline().ExecuteAsync(q, ctx, ct);]]></example>
public sealed class CodeAnalysisPipeline (
    IOutputWriter writer,
    IOptions<AppConfiguration> ConfigWrapper,
    [FromKeyedServices("Common")] ISystemPromptProvider
    systemPromptProvider,
    IResponseWriter SaveService,
    IOllamaClientFactory ollamaClientFactory,
    IOllamaResponse ollamaProvider)
    : ICodeAnalysisPipeline
{
    /// <summary>Runs full RAG pipeline: embed, retrieve, generate.</summary>
    /// <param name="question">User question.</param>
    /// <param name="contextText">Retrieved code context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <example><![CDATA[await pipeline.ExecuteAsync(question, ctx, ct);]]></example>
    public async Task ExecuteAsync (
      Question question,
      string contextText,
      CancellationToken ct)
    {
        var finalPrompt = $"Context:\n{contextText}\n\nQuestion:\n{question.Text}\n\nAnswer:";

        var ollamaClient = ollamaClientFactory.FindClient(OllamaServiceType.Ollama);

        var request = new GenerateRequest
        {
            Model = ollamaClient.SelectedModel,
            Prompt = finalPrompt,
            System = systemPromptProvider.Template,
        };

        var sw = Stopwatch.StartNew();

        var response = await GenerateAsync(request, ct);
        sw.Stop();

        if (ConfigWrapper.Value.RagOptions.IncludeOriginalPrompt)
        {
            response += finalPrompt.ShowPrompt();
        }

        await SaveResponseAsync(new SaveDetails(question, response, sw.ElapsedTimeString()), ct);
    }

    /// <summary>Saves LLM response to disk and logs path.</summary>
    /// <param name="details">Response details.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <example><![CDATA[await SaveResponseAsync(new SaveDetails(...), ct);]]></example>
    private async Task SaveResponseAsync (SaveDetails details, CancellationToken ct)
    {
        var path = await SaveService.WriteResponseAsync(details, ct);
        writer.WriteLine();
        writer.MarkupLine($"[red underline]{path}[/]");
    }

    /// <summary>Invokes Ollama generation and returns result.</summary>
    /// <param name="request">LLM prompt request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Generated text.</returns>
    /// <example><![CDATA[string answer = await GenerateAsync(request, ct);]]></example>
    private async ValueTask<string> GenerateAsync (GenerateRequest request, CancellationToken ct) => await ollamaProvider.GenerateResponse(request, ct);
}
