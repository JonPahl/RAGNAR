namespace Ragnar.Stages;

/// <summary>Renders Ragnar branding, tagline, and version to the console.</summary>
/// <remarks>First pipeline stage executed at application startup.</remarks>
/// <example><![CDATA[await stage.ExecuteAsync(ct);]]></example>
public class BrandingStage(IApplicationHeader header)
    : IPipelineStage
{

    /// <summary>Executes the branding render and returns a completed task.</summary>
    /// <param name="cancellationToken">Token to cancel the render (rarely used).</param>
    /// <remarks>Delegates to IApplicationHeader.RenderBranding synchronously.</remarks>
    /// <example><![CDATA[[await branding.ExecuteAsync(CancellationToken.None);]]></example>
    /// <returns>A completed Task indicating the stage finished.</returns>
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        header.RenderBranding();
        return Task.CompletedTask;
    }
}
