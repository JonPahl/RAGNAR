namespace Ragnar.Tests;

public class StageEntryTests
{
    public class TestCtx { }

    [Fact]
    public void ShouldStoreStageReference()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestCtx>>();
        stage.Setup(s => s.Name).Returns("Test");

        // Act
        var entry = new StageEntry<TestCtx>(stage.Object);

        // Assert
        Assert.Same(stage.Object, entry.Stage);
    }

    [Fact]
    public void PolicyShouldBeNullByDefault()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestCtx>>();

        // Act
        var entry = new StageEntry<TestCtx>(stage.Object);

        // Assert
        Assert.Null(entry.Policy);
    }

    [Fact]
    public void PolicyShouldBeSettable()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestCtx>>();
        var policy = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance.CreateLogger("test");
        // Use a real Polly policy for type correctness
        var retryPolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync([TimeSpan.FromSeconds(3)]);

        // Act
        var entry = new StageEntry<TestCtx>(stage.Object, retryPolicy);

        // Assert
        Assert.NotNull(entry.Policy);
    }

    [Fact]
    public void ShouldSupportRecordEquality()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestCtx>>();
        var entry1 = new StageEntry<TestCtx>(stage.Object);
        var entry2 = new StageEntry<TestCtx>(stage.Object);

        // Assert
        Assert.Equal(entry1, entry2);
    }

    [Fact]
    public void ShouldSupportWithExpression()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<TestCtx>>();
        var original = new StageEntry<TestCtx>(stage.Object);

        // Act
        var modified = original with { Policy = Polly.Policy.Handle<TimeoutException>().WaitAndRetryAsync([TimeSpan.FromSeconds(1)]) };

        // Assert
        Assert.Null(original.Policy);
        Assert.NotNull(modified.Policy);
        Assert.Same(stage.Object, modified.Stage);
    }
}
