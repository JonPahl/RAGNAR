namespace Ragnar.Stages;

public class BrandingStage(IApplicationHeader Header)
    : IPipelineStage
{
    public Task ExecuteAsync(CancellationToken Ct) => Task.Run(() => Header.RenderBranding(), Ct);
}
