namespace Ragnar.Abstractions;

public interface IPipelineStage
{
    Task ExecuteAsync(CancellationToken cancellationToken);
}
