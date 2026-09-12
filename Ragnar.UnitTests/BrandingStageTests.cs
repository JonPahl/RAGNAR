namespace Ragnar.Tests;

// ───────────────────────────────────────────────────────────────
//  3.  BrandingStage
// ───────────────────────────────────────────────────────────────
public class BrandingStageTests
{
    [Fact]
    public void BrandingStageImplementsIPipelineStage()
    {
        var mockHeader = new Mock<IApplicationHeader>();
        var stage = new BrandingStage(mockHeader.Object);

        Assert.IsAssignableFrom<IPipelineStage>(stage);
    }
}







