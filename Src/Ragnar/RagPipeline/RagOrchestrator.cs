namespace Ragnar.RagPipeline;

/// <summary>Executes RAG pipeline: embeds question → retrieve → generate answer.</summary>
/// <param name="logger">Serilog logger for diagnostic messages.</param>
/// <param name="writer">Custom console writer.</param>
/// <param name="configWrapper">Wraps all options.</param>
/// <param name="promptTemplateProvider">System prompt provider.</param>
/// <param name="saveService">Response writer service.</param>
/// <param name="ollamaClientFactory">OllamaOptions client factory.</param>
/// <param name="ollamaProvider">Ollama question calling operations.</param>
/// <example><![CDATA[await new RagPipeline().ExecuteAsync(q, ctx, ct);]]></example>
public sealed class RagOrchestrator(
    Serilog.ILogger logger,
    IOutputWriter writer,
    IOptions<RagnarConfig> configWrapper,
    [FromKeyedServices("Common")] IChatPromptProvider
    promptTemplateProvider,
    IRagPromptBuilder promptBuilder,
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
      Core.Models.Question question,
      string contextText,
      CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        var finalPrompt = promptBuilder
            .WithQuestion(question)
            .WithContext(contextText)
            .Build();

        //$"Context:\n{contextText}\n\nQuestion:\n{question.Text}\n\nAnswer:";

        var ollamaClient = ollamaClientFactory.ResolveClient(OllamaServiceType.Ollama);
        logger.Debug("Resolved Ollama client: {ClientType}, Model: {Model}", ollamaClient.GetType().Name, ollamaClient.SelectedModel);

        var generationOptions = configWrapper.Value.GenerationOptions; // or a dedicated IOptions<GenerationOptions>
        var request = new GenerateRequest
        {
            Model = ollamaClient.SelectedModel,
            Prompt = finalPrompt,
            System = promptTemplateProvider.System,
            Options = new RequestOptions
            {
                Temperature = generationOptions.Temperature,
                RepeatPenalty = generationOptions.RepeatPenalty,
            },
        };

        var response = string.Empty;

        try
        {
            response = await GenerateAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.Error(ex, "RAG generation failed for {Question}", question.Text);
            response = "⚠️  Generation failed. Check the Ollama service and retry.";
        }

        sw.Stop();

        logger.Information("RAG generation completed in {Elapsed}", sw.Elapsed);

        if (configWrapper.Value.ApplicationOptions.IncludeOriginalPrompt)
        {
            response += finalPrompt.ShowPrompt();
            response += Environment.NewLine;
            response += request.System;
            logger.Debug("Original prompt appended to response (IncludeOriginalPrompt = true).");
        }

        await SaveResponseAsync(new ResponseRecord(question, response, sw.FormatElapsedTime()), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Saves generation response to disk and prints the output path.</summary>
    /// <param name="details">Response details including question and generated text.</param>
    /// <param name="cancellationToken">Token to cancel the async write operation.</param>
    /// <returns>A completed task indicating successful persistence.</returns>
    /// <example><![CDATA[await SaveResponseAsync(new ResponseRecord(q, txt, "1.2s"), ct);]]></example>
    private async Task SaveResponseAsync(ResponseRecord details, CancellationToken cancellationToken)
    {
        logger.Debug("Saving response for question {QuestionId}", details.Question.Filename);
        var path = await saveService.WriteResponseAsync(details, cancellationToken)
            .ConfigureAwait(false);
        logger.Information("Response saved to: {Path}", path);
        writer.WriteLine();
        writer.MarkupLine($"[red underline]{path}[/]");
    }

    /// <summary>Invokes OllamaOptions generation with config.</summary>
    /// <param name="request">LLM prompt request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <example>
    /// <![CDATA[string answer = await GenerateAsync(request, ct);]]></example>
    /// <returns>Generated text.</returns>
    private async Task<string> GenerateAsync(GenerateRequest request, CancellationToken cancellationToken)
    {
        logger.Debug("Invoking Ollama generation with model {Model}", request.Model);

        var response = await ollamaProvider.GenerateResponse(request, cancellationToken).ConfigureAwait(false);
        logger.Debug("Ollama generation returned {Length} characters", response?.Length ?? 0);
        return response ?? "No response returned.";
    }
}
