namespace Ragnar.Abstractions;

public interface IEmbedTextPipeline
{
    Task RunAsync(CancellationToken cancellationToken);
}
