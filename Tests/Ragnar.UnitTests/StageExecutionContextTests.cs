namespace Ragnar.Tests;

public class StageExecutionContextTests
{
    [Fact]
    public void StageExecutionContextShouldStoreAllConstructorParameters()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<QuestionPipelineContext>>();
        stage.Setup(s => s.Name).Returns("TestStage");
        var ctx = new QuestionPipelineContext();

        // Act
        var execCtx = new StageExecutionContext<QuestionPipelineContext>(stage.Object, ctx, 2, 5);

        // Assert
        Assert.Same(stage.Object, execCtx.Stage);
        Assert.Same(ctx, execCtx.PipelineContext);
        Assert.Equal(2, execCtx.StageIndex);
        Assert.Equal(5, execCtx.StageTotal);
        Assert.Null(execCtx.Metric);
    }

    [Fact]
    public void StageExecutionContextShouldSupportMetricAssignment()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<QuestionPipelineContext>>();
        stage.Setup(s => s.Name).Returns("TestStage");
        var ctx = new QuestionPipelineContext();
        var execCtx = new StageExecutionContext<QuestionPipelineContext>(stage.Object, ctx, 0, 1);

        // Act
        var metric = new StageMetric { StageName = "Test", Duration = TimeSpan.FromSeconds(1), Success = true };
        execCtx.Metric = metric;

        // Assert
        Assert.Same(metric, execCtx.Metric);
    }

    [Fact]
    public void StageExecutionContextShouldSupportEqualityWhenAllFieldsMatch()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<QuestionPipelineContext>>();
        stage.Setup(s => s.Name).Returns("S");
        var ctx = new QuestionPipelineContext();

        var a = new StageExecutionContext<QuestionPipelineContext>(stage.Object, ctx, 1, 3);
        var b = new StageExecutionContext<QuestionPipelineContext>(stage.Object, ctx, 1, 3);

        // Assert
        Assert.Equal(a, b);
    }

    [Fact]
    public void StageExecutionContextShouldNotBeEqualWhenIndexDiffers()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<QuestionPipelineContext>>();
        stage.Setup(s => s.Name).Returns("S");
        var ctx = new QuestionPipelineContext();

        var a = new StageExecutionContext<QuestionPipelineContext>(stage.Object, ctx, 1, 3);
        var b = new StageExecutionContext<QuestionPipelineContext>(stage.Object, ctx, 2, 3);

        // Assert
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void StageExecutionContextShouldStoreAllConstructorValues()
    {
        var stage = new Mock<IPipelineStage<TestPipelineContext>>();
        stage.Setup(s => s.Name).Returns("TestStage");
        var ctx = new TestPipelineContext();

        var execCtx = new StageExecutionContext<TestPipelineContext>(stage.Object, ctx, 2, 5);

        Assert.Same(stage.Object, execCtx.Stage);
        Assert.Same(ctx, execCtx.PipelineContext);
        Assert.Equal(2, execCtx.StageIndex);
        Assert.Equal(5, execCtx.StageTotal);
        Assert.Null(execCtx.Metric);
    }

    [Fact]
    public void StageExecutionContextShouldSupportEquality()
    {
        var stage = new Mock<IPipelineStage<TestPipelineContext>>();
        var ctx = new TestPipelineContext();

        var a = new StageExecutionContext<TestPipelineContext>(stage.Object, ctx, 0, 3);
        var b = new StageExecutionContext<TestPipelineContext>(stage.Object, ctx, 0, 3);

        Assert.Equal(a, b);
    }

    [Fact]
    public void StageExecutionContextShouldDifferWhenIndexDiffers()
    {
        var stage = new Mock<IPipelineStage<TestPipelineContext>>();
        var ctx = new TestPipelineContext();

        var a = new StageExecutionContext<TestPipelineContext>(stage.Object, ctx, 0, 3);
        var b = new StageExecutionContext<TestPipelineContext>(stage.Object, ctx, 1, 3);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void StageExecutionContextShouldAllowSettingMetric()
    {
        var stage = new Mock<IPipelineStage<TestPipelineContext>>();
        var ctx = new TestPipelineContext();
        var execCtx = new StageExecutionContext<TestPipelineContext>(stage.Object, ctx, 0, 1);

        var metric = new StageMetric { StageName = "Test", Success = true };
        execCtx.Metric = metric;

        Assert.Same(metric, execCtx.Metric);
    }

}

internal class TestPipelineContext
{
}
