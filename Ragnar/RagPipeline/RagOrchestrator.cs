namespace Ragnar.RagPipeline;

/// <summary>Executes RAG pipeline: embeds question → retrieve → generate answer.</summary>
/// <param name="Writer">Custom console writer.</param>
/// <param name="ConfigWrapper">Wraps all options.</param>
/// <param name="SystemPromptProvider">System prompt provider.</param>
/// <param name="SaveService">Response writer service.</param>
/// <param name="OllamaClientFactory">OllamaOptions client factory.</param>
/// <param name="OllamaProvider">Ollama question calling operations.</param>
/// <example><![CDATA[await new RagPipeline().ExecuteAsync(q, ctx, ct);]]></example>
public sealed class RagOrchestrator(
    IOutputWriter Writer,
    IOptions<RagnarConfig> ConfigWrapper,
    [FromKeyedServices("Common")] ISystemPromptProvider
    SystemPromptProvider,
    IResponseWriter SaveService,
    IOllamaClientFactory OllamaClientFactory,
    IOllamaResponse OllamaProvider)
    : IRagOrchestrator
{
    /// <summary>Runs full RAG pipeline for a question using context.</summary>
    /// <param name="Question">User question.</param>
    /// <param name="ContextText">Retrieved code context.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <example><![CDATA[await pipeline.ExecuteAsync(question, ctx, ct);]]></example>
    /// <returns>Returns a task.</returns>
    public async Task ExecuteAsync(
      Question Question,
      string ContextText,
      CancellationToken Ct)
    {
        var finalPrompt = $"Context:\n{ContextText}\n\nQuestion:\n{Question.Text}\n\nAnswer:";

        var ollamaClient = OllamaClientFactory.FindClient(OllamaServiceType.Ollama);

        var request = new GenerateRequest
        {
            Model = ollamaClient.SelectedModel,
            Prompt = finalPrompt,
            System = SystemPromptProvider.Template,
        };

        var sw = Stopwatch.StartNew();

        var response = await GenerateAsync(request, Ct);
        sw.Stop();

        if (ConfigWrapper.Value.ApplicationOptions.IncludeOriginalPrompt)
        {
            response += finalPrompt.ShowPrompt();
        }

        await SaveResponseAsync(new SaveDetails(Question, response, sw.ElapsedTimeString()), Ct);
    }

    /// <summary>Saves generation response to disk and prints path.</summary>
    /// <param name="Details">Response details to save.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <example><![CDATA[await SaveResponseAsync(new SaveDetails(...), ct);]]></example>
    private async Task SaveResponseAsync(SaveDetails Details, CancellationToken Ct)
    {
        var path = await SaveService.WriteResponseAsync(Details, Ct);
        Writer.WriteLine();
        Writer.MarkupLine($"[red underline]{path}[/]");
    }

    /// <summary>Invokes OllamaOptions generation with config.</summary>
    /// <param name="Request">LLM prompt request.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <example>
    /// <![CDATA[string answer = await GenerateAsync(request, ct);]]></example>
    /// <returns>Generated text.</returns>
    private async ValueTask<string> GenerateAsync(GenerateRequest Request, CancellationToken Ct) => await OllamaProvider.GenerateResponse(Request, Ct);
}
