namespace Ragnar.Core.Utils;

/// <summary>
/// Provides extension methods to convert Ollama API clients into embedding generators.
/// </summary>
/// <example><![CDATA[[var gen = api.AsEmbeddingGenerator();]]></example>
public static class GeneratorExtensions
{
    /// <summary>
    /// Returns the Ollama API client as an <see cref="IEmbeddingGenerator{,}"/>,
    /// or throws if the client does not implement the interface.
    /// </summary>
    /// <exception cref="ArgumentNullException"><paramref name="api"/> is null.</exception>
    /// <exception cref="InvalidCastException">The client does not implement <see cref="IEmbeddingGenerator{TInput,TEmbedding}"/>.</exception>
    /// <example><![CDATA[var gen = ollamaClient.AsEmbeddingGenerator();]]>
    /// </example>
    public static IEmbeddingGenerator<string, Embedding<float>> AsEmbeddingGenerator(this IOllamaApiClient api)
    {
        Guard.Against.Null(api);

        return api is not IEmbeddingGenerator<string, Embedding<float>> generator
            ? throw new InvalidCastException(
                $"{nameof(IOllamaApiClient)} implementation ({api.GetType().Name}) " +
                "does not implement IEmbeddingGenerator<string, Embedding<float>>.")
            : generator;
    }
}
