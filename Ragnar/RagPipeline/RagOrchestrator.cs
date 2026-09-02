namespace Ragnar.RagPipeline;

/// <summary>Executes RAG pipeline: embeds question → retrieve → generate answer.</summary>
/// <param name="writer">Custom console writer.</param>
/// <param name="configWrapper">Wraps all options.</param>
/// <param name="promptTemplateProvider">System prompt provider.</param>
/// <param name="saveService">Response writer service.</param>
/// <param name="ollamaClientFactory">OllamaOptions client factory.</param>
/// <param name="ollamaProvider">Ollama question calling operations.</param>
/// <example><![CDATA[await new RagPipeline().ExecuteAsync(q, ctx, ct);]]></example>
public sealed class RagOrchestrator(
    IOutputWriter writer,
    IOptions<RagnarConfig> configWrapper,
    [FromKeyedServices("Common")] IPromptProvider
    promptTemplateProvider,
    IResponseWriter saveService,
    IOllamaClientFactory ollamaClientFactory,
    IOllamaGenerationService ollamaProvider)
    : IRagOrchestrator
{
    /// <summary>Runs full RAG pipeline for a question using context.</summary>
    /// <param name="question">User question.</param>
    /// <param name="contextText">Retrieved code context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <example><![CDATA[await pipeline.ExecuteAsync(question, ctx, ct);]]></example>
    /// <returns>Returns a task.</returns>
    public async Task ExecuteAsync(
      Core.Model.Question question,
      string contextText,
      CancellationToken cancellationToken)
    {
        var finalPrompt = $"Context:\n{contextText}\n\nQuestion:\n{question.Text}\n\nAnswer:";

        var ollamaClient = ollamaClientFactory.FindClient(OllamaServiceType.Ollama);

        var request = new GenerateRequest
        {
            Model = ollamaClient.SelectedModel,
            Prompt = finalPrompt,
            System = promptTemplateProvider.System,
        };

        var sw = Stopwatch.StartNew();

        var response = await GenerateAsync(request, cancellationToken);
        sw.Stop();

        if (configWrapper.Value.ApplicationOptions.IncludeOriginalPrompt)
        {
            response += finalPrompt.ShowPrompt();
            response += Environment.NewLine;
            response += request.System;
        }

        await SaveResponseAsync(new SaveDetails(question, response, sw.ElapsedTimeString()), cancellationToken);
    }

    /// <summary>Saves generation response to disk and prints path.</summary>
    /// <param name="details">Response details to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <example><![CDATA[await SaveResponseAsync(new SaveDetails(...), ct);]]></example>
    private async Task SaveResponseAsync(SaveDetails details, CancellationToken cancellationToken)
    {
        var path = await saveService.WriteResponseAsync(details, cancellationToken);
        writer.WriteLine();
        writer.MarkupLine($"[red underline]{path}[/]");
    }

    /// <summary>Invokes OllamaOptions generation with config.</summary>
    /// <param name="request">LLM prompt request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <example>
    /// <![CDATA[string answer = await GenerateAsync(request, ct);]]></example>
    /// <returns>Generated text.</returns>
    private async ValueTask<string> GenerateAsync(GenerateRequest request, CancellationToken cancellationToken) => await ollamaProvider.GenerateResponse(request, cancellationToken);
}
