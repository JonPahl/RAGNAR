namespace Ragnar.Core.Utils;

public static class GeneratorExtensions
{
    /// <summary>
    /// Converts an Ollama API client object into an embedding generator.
    /// </summary>
    /// <param name="Api">Ollama API object.</param>
    /// <returns>Embedding generator.</returns>
    public static IEmbeddingGenerator<string, Embedding<float>> AsEmbeddingGenerator(this IOllamaApiClient Api) => (IEmbeddingGenerator<string, Embedding<float>>)Api;
}
