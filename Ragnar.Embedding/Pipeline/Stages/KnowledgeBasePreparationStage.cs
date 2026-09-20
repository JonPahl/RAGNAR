namespace Ragnar.Embedding.Pipeline.Stages;

public class KnowledgeBasePreparationStage(IVectorSetup pipeline, IOutputWriter writer) : IPipelineStage<EmbeddingContext>
{
    public string Name => "Setup Qdrant Vector Store";

    public bool ShouldRun => true;

    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        await pipeline.EnsureCollectionExistsAsync(cancellationToken).ConfigureAwait(false);

        writer.MarkupLine("☑ Knowledge base populated.", new Style(ConsoleColor.Green));
    }
}
