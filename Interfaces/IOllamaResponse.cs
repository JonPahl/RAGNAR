namespace Ragnar.Interfaces;
/// <summary>
/// Call Ollama to generate a response to asked question.
/// </summary>
public interface IOllamaResponse
{
    /// <summary>Streams and collects full LLM response into a string.</summary>
    /// <param name="request">Generation request with prompt/options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Full generated text.</returns>
    /// <example><![CDATA[string answer = await provider.GenerateResponse(request, ct);]]></example>
    Task<string> GenerateResponse(GenerateRequest request, CancellationToken ct);
}
