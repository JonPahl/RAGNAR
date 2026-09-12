namespace Ragnar.Abstractions;

/// <summary>Interface for embedding generator service functionality.</summary>
/// <example><![CDATA[var v = await svc.GenerateAsync("hi", ct);]]></example>
public interface IEmbeddingService
{
    /// <summary>Generates a single vector embedding for the given input text via the Ollama model.</summary>
    /// <param name="input">The text to convert into a float vector.</param>
    /// <param name="ct">Token to cancel the embedding request.</param>
    /// <returns>A read-only memory of floats representing the embedding vector.</returns>
    /// <example><![CDATA[var vec = await svc.GenerateAsync("hello", ct);]]></example>
    Task<ReadOnlyMemory<float>> GenerateAsync(string input, CancellationToken ct);

    /// <summary>Generates embeddings for a batch of input strings in a single Ollama call.</summary>
    /// <param name="inputs">Collection of text strings to embed.</param>
    /// <param name="ct">Token to cancel the batch embedding request.</param>
    /// <returns>A GeneratedEmbeddings containing all vectors.</returns>
    /// <example><![CDATA[var batch = await svc.GenerateBatchAsync(list, ct);]]></example>
    Task<GeneratedEmbeddings<Embedding<float>>> GenerateBatchAsync(IReadOnlyCollection<string> inputs, CancellationToken ct);
}

