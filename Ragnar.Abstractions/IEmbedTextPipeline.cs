namespace Ragnar.Abstractions;

/// <summary>Contract for the end-to-end embed-and-store text pipeline.</summary>
/// <example><![CDATA[await sp.GetRequiredService<IEmbedTextPipeline>().RunAsync(ct);]]></example>
public interface IEmbedTextPipeline
{
    /// <summary>Executes discovery, parsing, embedding, and Qdrant upsertion.</summary>
    /// <param name="cancellationToken">Token to abort the full pipeline in flight.</param>
    /// <returns>A task representing pipeline completion.</returns>
    /// <example><![CDATA[await pipeline.RunAsync(CancellationToken.None);]]></example>
    Task RunAsync(CancellationToken cancellationToken);
}
