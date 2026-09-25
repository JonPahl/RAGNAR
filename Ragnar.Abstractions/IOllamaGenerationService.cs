namespace Ragnar.Abstractions;

/// <summary>Streams Ollama LLM chat responses with console rendering.</summary>
/// <example><![CDATA[var txt = await svc.GenerateResponse(req, ct);]]></example>
public interface IOllamaGenerationService
{
    /// <summary>Generates a full chat response using configured Ollama options.</summary>
    /// <param name="request">The generation request with prompt and system text.</param>
    /// <param name="cancellationToken">Token to abort generation.</param>
    /// <returns>The complete generated text response string.</returns>
    /// <example><![CDATA[string t = await svc.GenerateResponse(req, ct);]]></example>
    Task<string> GenerateResponse(GenerateRequest request, CancellationToken cancellationToken);
}
