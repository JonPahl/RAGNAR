namespace Ragnar.Core.Interface;

public interface IPipelineStage
{
    Task ExecuteAsync(CancellationToken Ct);
}
