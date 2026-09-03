namespace Ragnar.Embedding.Pipeline.Stages;

public class KnowledgeBasePreparationStage(IEmbeddingPipeline pipeline, IOutputWriter writer)
    : IPipelineStage
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await pipeline.EnsureCollectionExistsAsync(cancellationToken);
        await pipeline.PopulateAsync(cancellationToken);
        writer.MarkupLine("☑ Knowledge base populated.", new Style(ConsoleColor.Green));
    }
}
