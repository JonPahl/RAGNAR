namespace Ragnar.Embedding.Pipeline.Services;

/// <summary>Initializes a new instance of the embedding service.</summary>
/// <param name ="Logger"> Serilog logger for tracking errors and info.</param>
/// <param name ="ClientFactory"> Factory responsible for creating Ollama clients.</param>
public class OllamaEmbeddingService(
    Serilog.ILogger Logger,
    IOllamaClientFactory ClientFactory)
        : IEmbeddingService
{
    /// <summary>Generates vector embeddings for the provided input text.</summary>
    /// <param name = "Input"> Text to embed into a vector representation.</param>
    /// <param name = "Ct"> Cancellation token to abort the operation.</param>
    /// <returns>A read-only memory buffer containing float vectors.</returns>
    /// <example><![CDATA[var vec = await service.GenerateAsync("hello", ct);]]></example>
    public async Task<ReadOnlyMemory<float>> GenerateAsync(string Input, CancellationToken Ct)
    {
        var Generator = ClientFactory.FindClient(OllamaServiceType.Embedding).AsEmbeddingGenerator();

        var Embeddings = await Generator.GenerateAsync(Input, cancellationToken: Ct);

        return Embeddings.Vector;
    }
}
