namespace Ragnar.Embedding.Pipeline.Stages;

public class QuestionProcessingStage(IKnowledgeBaseInitialize knowledgeBase)
        : IPipelineStage
{
    public Task ExecuteAsync(CancellationToken cancellationToken) => knowledgeBase.AskQuestionsAsync(cancellationToken);
}
