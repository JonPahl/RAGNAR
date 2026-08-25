namespace Ragnar.Core.Interface;

public interface IEmbedTextPipeline
{
    Task RunAsync(CancellationToken Ct);
}
