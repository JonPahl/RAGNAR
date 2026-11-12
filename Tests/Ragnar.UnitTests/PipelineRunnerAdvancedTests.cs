namespace Ragnar.Tests;

public class PipelineRunnerAdvancedTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger> _loggerMock;
    private readonly PipelineRunner<QuestionPipelineContext> _sut;

    public PipelineRunnerAdvancedTests()
    {
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
        _sut = new PipelineRunner<QuestionPipelineContext>(_loggerMock.Object);
    }

    #region AddStage

    [Fact]
    public void AddStageShouldReturnRunnerForChaining()
    {
        var stage = CreateMockStage("Stage1");
        var result = _sut.AddStage(stage.Object);
        Assert.Same(_sut, result);
    }

    [Fact]
    public void AddStageWithNullShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.AddStage(null!));
    }

    [Fact]
    public void AddDuplicateStageShouldThrow()
    {
        var stage = CreateMockStage("Duplicate");
        _sut.AddStage(stage.Object);

        var duplicate = CreateMockStage("Duplicate");
        Assert.Throws<ArgumentException>(() => _sut.AddStage(duplicate.Object));
    }

    #endregion

    #region ClearStages

    [Fact]
    public void ClearStagesShouldRemoveAllStages()
    {
        _sut.AddStage(CreateMockStage("A").Object)
            .AddStage(CreateMockStage("B").Object);

        var result = _sut.ClearStages();

        Assert.Empty(_sut.StageNames);
        Assert.Same(_sut, result);
    }

    #endregion

    #region ReplaceStage

    [Fact]
    public void ReplaceStageShouldReplaceExistingStage()
    {
        var oldStage = CreateMockStage("Original");
        var newStage = CreateMockStage("Replacement");
        _sut.AddStage(oldStage.Object);

        _sut.ReplaceStage("Original", newStage.Object);

        Assert.Contains("Replacement", _sut.StageNames);
        Assert.DoesNotContain("Original", _sut.StageNames);
    }

    #endregion

    #region RemoveStage

    [Fact]
    public void RemoveStageShouldRemoveByName()
    {
        _sut.AddStage(CreateMockStage("ToRemove").Object)
            .AddStage(CreateMockStage("Keep").Object);

        _sut.RemoveStage("ToRemove");

        Assert.DoesNotContain("ToRemove", _sut.StageNames);
        Assert.Contains("Keep", _sut.StageNames);
    }

    [Fact]
    public void RemoveStageWithNonExistentNameShouldNotThrow()
    {
        _sut.AddStage(CreateMockStage("Existing").Object);

        _sut.RemoveStage("NonExistent");

        Assert.Contains("Existing", _sut.StageNames);
    }

    #endregion

    #region Reorder

    [Fact]
    public void ReorderShouldMoveStageToNewPosition()
    {
        _sut.AddStage(CreateMockStage("First").Object)
            .AddStage(CreateMockStage("Second").Object)
            .AddStage(CreateMockStage("Third").Object);

        _sut.Reorder("Third", 0);

        Assert.Equal(["Third", "First", "Second"], _sut.StageNames);
    }

    [Fact]
    public void ReorderWithInvalidNameShouldThrow()
    {
        _sut.AddStage(CreateMockStage("Existing").Object);

        Assert.Throws<KeyNotFoundException>(() => _sut.Reorder("NonExistent", 0));
    }

    [Fact]
    public void ReorderShouldClampIndexToValidRange()
    {
        _sut.AddStage(CreateMockStage("A").Object)
            .AddStage(CreateMockStage("B").Object);

        _sut.Reorder("B", 999);

        Assert.Equal(2, _sut.StageNames.Count);
    }

    #endregion

    #region ExecuteAsync

    [Fact]
    public async Task ExecuteAsyncShouldRunAllStagesInOrder()
    {
        var order = new List<string>();
        _sut.AddStage(CreateMockStage("S1", order).Object)
            .AddStage(CreateMockStage("S2", order).Object)
            .AddStage(CreateMockStage("S3", order).Object);

        var ctx = new QuestionPipelineContext();
        await _sut.ExecuteAsync(ctx, CancellationToken.None);

        Assert.Equal(new[] { "S1", "S2", "S3" }, order);
    }

    [Fact]
    public async Task ExecuteAsyncShouldSkipStagesThatShouldNotRun()
    {
        var order = new List<string>();
        var skipStage = CreateMockStage("Skip", order, shouldRun: false).Object;
        _sut.AddStage(CreateMockStage("Run", order).Object)
            .AddStage(skipStage)
            .AddStage(CreateMockStage("Run2", order).Object);

        await _sut.ExecuteAsync(new QuestionPipelineContext(), CancellationToken.None);

        Assert.Equal(new[] { "Run", "Run2" }, order);
    }

    [Fact]
    public async Task ExecuteAsyncShouldThrowOnStageFailure()
    {
        var failingStage = new Mock<IPipelineStage<QuestionPipelineContext>>();
        failingStage.Setup(s => s.Name).Returns("Failing");
        failingStage.Setup(s => s.ShouldRun(It.IsAny<QuestionPipelineContext>())).Returns(true);
        failingStage.Setup(s => s.ExecuteAsync(It.IsAny<QuestionPipelineContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));
        _sut.AddStage(failingStage.Object);

        await Assert.ThrowsAsync<PipelineStageException>(
            () => _sut.ExecuteAsync(new QuestionPipelineContext(), CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsyncShouldPropagateCancellation()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var cancelStage = new Mock<IPipelineStage<QuestionPipelineContext>>();
        cancelStage.Setup(s => s.Name).Returns("Cancel");
        cancelStage.Setup(s => s.ShouldRun(It.IsAny<QuestionPipelineContext>())).Returns(true);
        cancelStage.Setup(s => s.ExecuteAsync(It.IsAny<QuestionPipelineContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());
        _sut.AddStage(cancelStage.Object);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.ExecuteAsync(new QuestionPipelineContext(), cts.Token));
    }

    #endregion

    #region StageNames

    [Fact]
    public void StageNamesShouldReturnEmptyWhenNoStages()
    {
        Assert.Empty(_sut.StageNames);
    }

    [Fact]
    public void StageNamesShouldReturnOrderedNames()
    {
        _sut.AddStage(CreateMockStage("B").Object)
            .AddStage(CreateMockStage("A").Object);

        Assert.Equal(["B", "A"], _sut.StageNames);
    }

    #endregion

    #region Helpers

    private static Mock<IPipelineStage<QuestionPipelineContext>> CreateMockStage(
        string name,
        List<string>? executionOrder = null,
        bool shouldRun = true)
    {
        var mock = new Mock<IPipelineStage<QuestionPipelineContext>>();
        mock.Setup(s => s.Name).Returns(name);
        AsyncRetryPolicy? x = null;

        mock.Setup(s => s.RetryPolicy).Returns(x);
        mock.Setup(s => s.ShouldRun(It.IsAny<QuestionPipelineContext>())).Returns(shouldRun);
        mock.Setup(s => s.ExecuteAsync(It.IsAny<QuestionPipelineContext>(), It.IsAny<CancellationToken>()))
            .Callback<QuestionPipelineContext, CancellationToken>((_, _) => executionOrder?.Add(name))
            .Returns(Task.CompletedTask);
        return mock;
    }

    #endregion
}
