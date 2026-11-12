namespace Ragnar.Tests;

public sealed class TestStage(
    string name,
    Func<PipelineTestContext, Task> action,
    bool shouldRun = true,
    AsyncRetryPolicy? retryPolicy = null) : IPipelineStage<PipelineTestContext>
{
    public string Name { get; } = name;
    public AsyncRetryPolicy? RetryPolicy { get; } = retryPolicy;
    public bool ShouldRun(PipelineTestContext context) => shouldRun;
    public Task ExecuteAsync(PipelineTestContext context, CancellationToken ct) => action(context);
}
