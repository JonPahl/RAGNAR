using Microsoft.Extensions.AI;

using OllamaSharp;

namespace Ragnar.Core.Utils;

public static class GeneratorExtensions
{
    /// <summary>
    /// Converts an Ollama API client object into an embedding generator.
    /// </summary>
    /// <param name="api">Ollama API object.</param>
    /// <returns>Embedding generator.</returns>
    public static IEmbeddingGenerator<string, Embedding<float>> AsEmbeddingGenerator(this IOllamaApiClient api) => (IEmbeddingGenerator<string, Embedding<float>>)api;
}
