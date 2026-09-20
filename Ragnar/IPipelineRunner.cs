namespace Ragnar;

public interface IPipelineRunner
{
    PipelineRunner AddStage(IPipelineStage<EmbeddingContext> stage);
    Task<EmbeddingContext> ExecuteAsync(EmbeddingContext context, CancellationToken ct);
}