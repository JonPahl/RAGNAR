namespace Ragnar.Embedding.Pipeline;

public class RagPipelineRunner([FromKeyedServices("Main")] IEnumerable<IPipelineStage> stages)
        : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var stage in stages) await stage.ExecuteAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
