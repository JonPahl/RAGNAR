namespace Ragnar.RagPipeline;

/// <summary>Executes RAG pipeline: embeds question → retrieve → generate answer.</summary>
/// <param name="Writer">Custom console writer.</param>
/// <param name="ConfigWrapper">Wraps all options.</param>
/// <param name="SystemPromptProvider">System prompt provider.</param>
/// <param name="SaveService">Response writer service.</param>
/// <param name="OllamaClientFactory">OllamaOptions client factory.</param>
/// <param name="OllamaProvider">Ollama question calling operations.</param>
/// <example><![CDATA[await new RagPipeline().ExecuteAsync(q, ctx, ct);]]></example>
public sealed class CodeAnalysisPipeline(
    IOutputWriter Writer,
    IOptions<AppConfiguration> ConfigWrapper,
    [FromKeyedServices("Common")] ISystemPromptProvider
    SystemPromptProvider,
    IResponseWriter SaveService,
    IOllamaClientFactory OllamaClientFactory,
    IOllamaResponse OllamaProvider)
    : ICodeAnalysisPipeline
{
    /// <summary>Runs full RAG pipeline: embed, retrieve, generate.</summary>
    /// <param name="Question">User question.</param>
    /// <param name="ContextText">Retrieved code context.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <example><![CDATA[await pipeline.ExecuteAsync(question, ctx, ct);]]></example>
    public async Task ExecuteAsync(
      Question Question,
      string ContextText,
      CancellationToken Ct)
    {
        var FinalPrompt = $"Context:\n{ContextText}\n\nQuestion:\n{Question.Text}\n\nAnswer:";

        var OllamaClient = OllamaClientFactory.FindClient(OllamaServiceType.Ollama);

        var Request = new GenerateRequest
        {
            Model = OllamaClient.SelectedModel,
            Prompt = FinalPrompt,
            System = SystemPromptProvider.Template,
        };

        var Sw = Stopwatch.StartNew();

        var Response = await GenerateAsync(Request, Ct);
        Sw.Stop();

        if(ConfigWrapper.Value.RagOptions.IncludeOriginalPrompt)
        {
            Response += FinalPrompt.ShowPrompt();
        }

        await SaveResponseAsync(new SaveDetails(Question, Response, Sw.ElapsedTimeString()), Ct);
    }

    /// <summary>Saves LLM response to disk and logs path.</summary>
    /// <param name="Details">Response details.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <example><![CDATA[await SaveResponseAsync(new SaveDetails(...), ct);]]></example>
    private async Task SaveResponseAsync(SaveDetails Details, CancellationToken Ct)
    {
        var Path = await SaveService.WriteResponseAsync(Details, Ct);
        Writer.WriteLine();
        Writer.MarkupLine($"[red underline]{Path}[/]");
    }

    /// <summary>Invokes Ollama generation and returns result.</summary>
    /// <param name="Request">LLM prompt request.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Generated text.</returns>
    /// <example><![CDATA[string answer = await GenerateAsync(request, ct);]]></example>
    private async ValueTask<string> GenerateAsync(GenerateRequest Request, CancellationToken Ct) => await OllamaProvider.GenerateResponse(Request, Ct);
}
