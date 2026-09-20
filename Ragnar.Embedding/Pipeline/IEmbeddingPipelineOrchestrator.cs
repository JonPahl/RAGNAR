namespace Ragnar.Embedding.Pipeline;

public interface IEmbeddingPipelineOrchestrator
{
    Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken);
}
