namespace Ragnar.Builder;

public interface IDocumentProcessingPipeline
{
    ValueTask RunAsync(CancellationToken cancellationToken = default);
}
