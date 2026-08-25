namespace Ragnar.Stages.Questions;

public class QuestionOneStage : IPipelineStage
{
    public Task ExecuteAsync(CancellationToken Ct)
    {
        var fcl = new FileConfigLoader();
        var configs = new List<QuestionConfiguration>();
        configs.AddRange(fcl.LoadQuestions());

        return Task.CompletedTask;
    }
}
