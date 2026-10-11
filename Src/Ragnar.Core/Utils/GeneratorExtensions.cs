namespace Ragnar.Core.Utils;

/// <summary>
/// Provides extension methods to convert Ollama API clients into embedding generators.
/// </summary>
/// <example><![CDATA[[var gen = api.AsEmbeddingGenerator();]]></example>
public static class GeneratorExtensions
{
    /// <summary>
    /// Converts an Ollama API client object into an embedding generator.
    /// </summary>
    /// <param name="api">Ollama API object.</param>
    /// <returns>Embedding generator.</returns>
    /// <example><![CDATA[var gen = ollamaClient.AsEmbeddingGenerator();]]></example>
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
