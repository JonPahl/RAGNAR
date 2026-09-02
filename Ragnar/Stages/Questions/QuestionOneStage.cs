namespace Ragnar.Stages.Questions;

public class QuestionOneStage
    : IPipelineStage
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var fcl = new FileConfigLoader();
        var configs = new List<Plugins.Question>();
        configs.AddRange(fcl.LoadQuestions());

        return Task.CompletedTask;
    }
}
