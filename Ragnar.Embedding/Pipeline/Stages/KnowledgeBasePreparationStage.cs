namespace Ragnar.Embedding.Pipeline.Stages;

public class KnowledgeBasePreparationStage(IEmbeddingPipeline Pipeline, IOutputWriter Writer)
    : IPipelineStage
{
    public async Task ExecuteAsync(CancellationToken Ct)
    {
        await Pipeline.EnsureCollectionExistsAsync(Ct);
        await Pipeline.PopulateAsync(Ct);
        Writer.MarkupLine("☑ Knowledge base populated.", new Style(ConsoleColor.Green));
    }
}
