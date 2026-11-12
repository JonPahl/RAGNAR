namespace Ragnar.Tests;

/// <summary>
/// Integration tests for PipelineRunner stage management and execution.
/// </summary>
public class PipelineRunnerIntegrationTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger> _loggerMock;
    private readonly PipelineRunner<QuestionPipelineContext> _runner;

    public PipelineRunnerIntegrationTests()
    {
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
        _runner = new PipelineRunner<QuestionPipelineContext>(_loggerMock.Object);
    }

    private sealed class TestStage(string name, bool shouldRun = true, bool throwOnExecute = false) : IPipelineStage<QuestionPipelineContext>
    {
        public string Name { get; } = name;
        public AsyncRetryPolicy? RetryPolicy => null;
        public bool ShouldRun(QuestionPipelineContext context) => shouldRun;

        public Task ExecuteAsync(QuestionPipelineContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (throwOnExecute)
                throw new InvalidOperationException($"{Name} failed");
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void AddStageShouldRegisterStage()
    {
        var stage = new TestStage("TestStage");
        var result = _runner.AddStage(stage);

        Assert.Same(_runner, result);
        Assert.Contains("TestStage", _runner.StageNames);
    }

    [Fact]
    public void AddStageWithDuplicateNameShouldThrow()
    {
        _runner.AddStage(new TestStage("Duplicate"));

        Assert.Throws<ArgumentException>(() => _runner.AddStage(new TestStage("Duplicate")));
    }

    [Fact]
    public void AddStageWithNullShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => _runner.AddStage(null!));
    }
}
