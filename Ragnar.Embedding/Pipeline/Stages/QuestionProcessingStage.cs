namespace Ragnar.Embedding.Pipeline.Stages;

public class QuestionProcessingStage(IKnowledgeBaseInitialize KnowledgeBase)
        : IPipelineStage
{
    public Task ExecuteAsync(CancellationToken Ct) => KnowledgeBase.AskQuestionsAsync(Ct);
}
