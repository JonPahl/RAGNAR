namespace Ragnar.Tests;

public class StageEntryTests
{
    [Fact]
    public void StageEntryShouldExposePipelineStage()
    {
        var stage = new Mock<IPipelineStage<object>>();
        stage.Setup(s => s.Name).Returns("MyStage");

        var entry = new StageEntry<object>(stage.Object);

        Assert.Same(stage.Object, entry.PipelineStage);
    }

    [Fact]
    public void StageEntryShouldExposeNullPolicyByDefault()
    {
        var stage = new Mock<IPipelineStage<object>>();

        var entry = new StageEntry<object>(stage.Object);

        Assert.Null(entry.AsyncPolicy);
    }

    [Fact]
    public void StageEntryShouldThrowOnNullStage()
    {
        Assert.Throws<ArgumentNullException>(() => new StageEntry<object>(null!));
    }
}
