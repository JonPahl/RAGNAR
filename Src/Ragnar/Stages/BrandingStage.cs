namespace Ragnar.Stages;

/// <summary>Renders Ragnar branding, tagline, and version to the console.</summary>
/// <param name="header">Application header service that draws the banner.</param>
/// <remarks>First pipeline stage executed at application startup.</remarks>
/// <example><![CDATA[await stage.ExecuteAsync(ct);]]></example>
public class BrandingStage(IApplicationHeader header)
    : IPipelineStage<EmbeddingContext>
{

    /// <summary>Gets the human-readable name of this branding pipeline stage.</summary>
    /// <returns>The string "Branding Stage".</returns>
    /// <example><![CDATA[string n = stage.Name;]]>
    /// </example>
    public string Name => "Branding Stage";

    /// <summary>Always null; this display stage requires no retry logic.</summary>
    public AsyncRetryPolicy? RetryPolicy => null;

    /// <summary>Indicates the branding stage should always execute in the pipeline.</summary>
    /// <returns>Always true, ensuring the banner renders on every run.</returns>
    /// <example><![CDATA[bool run = stage.ShouldRun(ctx); // true]]></example>
    public bool ShouldRun(EmbeddingContext context) => true;

    /// <summary>Renders the Ragnar banner, tagline, and version to the console output.</summary>
    /// <param name="context">The shared embedding pipeline context (unused).</param>
    /// <param name="cancellationToken">Token to abort rendering if the user cancels.</param>
    /// <returns>A completed task once the branding is rendered.</returns>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    public Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        header.RenderBranding();
        return Task.CompletedTask;
    }
}
