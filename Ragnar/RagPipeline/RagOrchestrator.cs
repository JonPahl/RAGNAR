
using System.Diagnostics;

namespace Ragnar.RagPipeline;

/// <summary>Executes RAG pipeline: embeds question → retrieve → generate answer.</summary>
/// <param name="writer">Custom console writer.</param>
/// <param name="ConfigWrapper">Wraps all options.</param>
/// <param name="systemPromptProvider">System prompt provider.</param>
/// <param name="SaveService">Response writer service.</param>
/// <param name="ollamaClientFactory">OllamaOptions client factory.</param>
/// <param name="ollamaProvider">Ollama question calling operations.</param>
/// <example><![CDATA[await new RagPipeline().RunAsync(q, ctx, ct);]]></example>
public sealed class RagOrchestrator(
    IOutputWriter writer,
    IOptions<ApplicationConfiguration> ConfigWrapper,
    [FromKeyedServices("Common")] ISystemPromptProvider
    systemPromptProvider,
    IResponseWriter SaveService,
    IOllamaClientProvider ollamaClientFactory,
    IOllamaResponse ollamaProvider)
    : IRagOrchestrator
{
    /// <summary>Runs full RAG pipeline for a question using context.</summary>
    /// <param name="question">User question.</param>
    /// <param name="contextText">Retrieved code context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <example><![CDATA[await pipeline.RunAsync(question, ctx, ct);]]></example>
    /// <returns>Returns a task.</returns>
    public async Task RunAsync(
      Question question,
      string contextText,
      CancellationToken ct)
    {
        var finalPrompt = $"Context:\n{contextText}\n\nQuestion:\n{question.Text}\n\nAnswer:";

        var ollamaClient = ollamaClientFactory.FindClient(OllamaType.Ollama);

        var request = new GenerateRequest
        {
            Model = ollamaClient.SelectedModel,
            Prompt = finalPrompt,
            System = systemPromptProvider.Template,
        };

        var sw = Stopwatch.StartNew();

        var response = await GenerateAsync(request, ct);
        sw.Stop();

        if (ConfigWrapper.Value.ApplicationOptions.IncludeOriginalPrompt)
        {
            finalPrompt.ShowPrompt();
        }

        await SaveResponseAsync(new SaveDetails(question, response, sw.ElapsedTimeString()), ct);
    }

    /// <summary>Saves generation response to disk and prints path.</summary>
    /// <param name="details">Response details to save.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <example><![CDATA[await SaveResponseAsync(new SaveDetails(...), ct);]]></example>
    private async Task SaveResponseAsync(SaveDetails details, CancellationToken ct)
    {
        var path = await SaveService.WriteResponseAsync(details, ct);
        writer.WriteLine();
        writer.MarkupLine($"[red underline]{path}[/]");
    }

    /// <summary>Invokes OllamaOptions generation with config.</summary>
    /// <param name="request">LLM prompt request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <example>
    /// <![CDATA[string answer = await GenerateAsync(request, ct);]]></example>
    /// <returns>Generated text.</returns>
    private async ValueTask<string> GenerateAsync(GenerateRequest request, CancellationToken ct) => await ollamaProvider.GenerateResponse(request, ct);
}
