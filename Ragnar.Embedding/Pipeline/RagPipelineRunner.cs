namespace Ragnar.Embedding.Pipeline;

public class RagPipelineRunner([FromKeyedServices("Main")] IEnumerable<IPipelineStage> Stages)
        : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var stage in Stages) await stage.ExecuteAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
