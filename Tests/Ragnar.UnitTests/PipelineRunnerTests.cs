//namespace Ragnar.Tests;

//public class PipelineRunnerTests
//{
//    private readonly Mock<Serilog.ILogger> _loggerMock;
//    private readonly PipelineRunner<TestContext> _sut;

//    public PipelineRunnerTests()
//    {
//        _loggerMock = new Mock<Serilog.ILogger>();
//        _sut = new PipelineRunner<TestContext>(_loggerMock.Object);
//    }

//    // --- AddStage ---

//    [Fact]
//    public void AddStageShouldRegisterStageAndReturnThis()
//    {
//        // Arrange
//        var stage = CreateMockStage("TestStage");

//        // Act
//        var result = _sut.AddStage(stage.Object);

//        // Assert
//        Assert.Same(_sut, result);
//        Assert.Contains("TestStage", _sut.StageNames);
//    }

//    [Fact]
//    public void AddStageWithNullShouldThrowArgumentNullException()
//    {
//        // Act & Assert
//        Assert.ThrowsAny<ArgumentNullException>(() => _sut.AddStage(null!));
//    }

//    [Fact]
//    public void AddStageWithDuplicateNameShouldThrowArgumentException()
//    {
//        // Arrange
//        var stage1 = CreateMockStage("Duplicate");
//        _sut.AddStage(stage1.Object);

//        // Act & Assert
//        Assert.Throws<ArgumentException>(() => _sut.AddStage(CreateMockStage("Duplicate").Object));
//    }

//    [Fact]
//    public void AddStageWithPolicyShouldRegister()
//    {
//        // Arrange
//        var stage = CreateMockStage("WithPolicy");
//        var policy = Policy.NoOpAsync();

//        // Act
//        var result = _sut.AddStage(stage.Object, policy);

//        // Assert
//        Assert.Same(_sut, result);
//        Assert.Contains("WithPolicy", _sut.StageNames);
//    }

//    // --- RemoveStage ---

//    [Fact]
//    public void RemoveStageShouldRemoveRegisteredStage()
//    {
//        // Arrange
//        _sut.AddStage(CreateMockStage("ToRemove").Object);
//        Assert.Contains("ToRemove", _sut.StageNames);

//        // Act
//        var result = _sut.RemoveStage("ToRemove");

//        // Assert
//        Assert.Same(_sut, result);
//        Assert.DoesNotContain("ToRemove", _sut.StageNames);
//    }

//    [Fact]
//    public void RemoveStageWithNonExistentNameShouldNotThrow()
//    {
//        // Act
//        var result = _sut.RemoveStage("NonExistent");

//        // Assert
//        Assert.Same(_sut, result);
//    }

//    // --- Reorder ---

//    [Fact]
//    public void ReorderShouldMoveStageToNewPosition()
//    {
//        // Arrange
//        _sut.AddStage(CreateMockStage("First").Object);
//        _sut.AddStage(CreateMockStage("Second").Object);
//        _sut.AddStage(CreateMockStage("Third").Object);

//        // Act
//        _sut.Reorder("Third", 0);

//        // Assert
//        var names = _sut.StageNames;
//        Assert.Equal("Third", names[0]);
//        Assert.Equal("First", names[1]);
//        Assert.Equal("Second", names[2]);
//    }

//    [Fact]
//    public void ReorderWithNonExistentNameShouldThrowKeyNotFoundException()
//    {
//        // Arrange
//        _sut.AddStage(CreateMockStage("Existing").Object);

//        // Act & Assert
//        Assert.Throws<KeyNotFoundException>(() => _sut.Reorder("Missing", 0));
//    }

//    [Fact]
//    public void ReorderWithOutOfRangeIndexShouldClamp()
//    {
//        // Arrange
//        _sut.AddStage(CreateMockStage("A").Object);
//        _sut.AddStage(CreateMockStage("B").Object);

//        // Act
//        _sut.Reorder("A", 100); // clamped to end

//        // Assert
//        var names = _sut.StageNames;
//        Assert.Equal("A", names[^1]);
//    }

//    [Fact]
//    public void ReorderWithNegativeIndexShouldClampToZero()
//    {
//        // Arrange
//        _sut.AddStage(CreateMockStage("A").Object);
//        _sut.AddStage(CreateMockStage("B").Object);

//        // Act
//        _sut.Reorder("B", -5); // clamped to 0

//        // Assert
//        Assert.Equal("B", _sut.StageNames[0]);
//    }

//    // --- ReplaceStage ---

//    [Fact]
//    public void ReplaceStageShouldSwapStage()
//    {
//        // Arrange
//        _sut.AddStage(CreateMockStage("Old").Object);

//        // Act
//        _sut.ReplaceStage("Old", CreateMockStage("New").Object);

//        // Assert
//        Assert.DoesNotContain("Old", _sut.StageNames);
//        Assert.Contains("New", _sut.StageNames);
//    }

//    // --- ClearStages ---

//    [Fact]
//    public void ClearStagesShouldRemoveAllStages()
//    {
//        // Arrange
//        _sut.AddStage(CreateMockStage("A").Object);
//        _sut.AddStage(CreateMockStage("B").Object);

//        // Act
//        var result = _sut.ClearStages();

//        // Assert
//        Assert.Same(_sut, result);
//        Assert.Empty(_sut.StageNames);
//    }

//    // --- ExecuteAsync ---

//    [Fact]
//    public async Task ExecuteAsyncShouldExecuteAllStagesInOrder()
//    {
//        // Arrange
//        var executionOrder = new List<string>();
//        _sut.AddStage(CreateExecutingStage("First", executionOrder).Object);
//        _sut.AddStage(CreateExecutingStage("Second", executionOrder).Object);
//        var context = new TestContext();

//        // Act
//        var result = await _sut.ExecuteAsync(context, CancellationToken.None);

//        // Assert
//        Assert.Same(context, result);
//        Assert.Equal(new[] { "First", "Second" }, executionOrder);
//    }

//    [Fact]
//    public async Task ExecuteAsyncShouldSkipStageWhenShouldRunReturnsFalse()
//    {
//        // Arrange
//        var executionOrder = new List<string>();
//        _sut.AddStage(CreateNonRunningStage("Skipped").Object);
//        _sut.AddStage(CreateExecutingStage("Executed", executionOrder).Object);
//        var context = new TestContext();

//        // Act
//        await _sut.ExecuteAsync(context, CancellationToken.None);

//        // Assert
//        Assert.Single(executionOrder);
//        Assert.Equal("Executed", executionOrder[0]);
//    }

//    [Fact]
//    public async Task ExecuteAsyncWithCancelledTokenShouldThrow()
//    {
//        // Arrange
//        using var cts = new CancellationTokenSource();
//        await cts.CancelAsync();
//        var stage = CreateExecutingStage("Cancelled", []).Object;
//        _sut.AddStage(stage);
//        var context = new TestContext();

//        // Act & Assert
//        await Assert.ThrowsAnyAsync<OperationCanceledException>(
//            () => _sut.ExecuteAsync(context, cts.Token));
//    }

//    [Fact]
//    public async Task ExecuteAsyncWithFailingStageShouldThrowPipelineStageException()
//    {
//        // Arrange
//        var failingStage = new Mock<IPipelineStage<TestContext>>();
//        failingStage.Setup(s => s.Name).Returns("Failing");
//        failingStage.Setup(s => s.ShouldRun(It.IsAny<TestContext>())).Returns(true);
//        failingStage.Setup(s => s.ExecuteAsync(It.IsAny<TestContext>(), It.IsAny<CancellationToken>()))
//                   .ThrowsAsync(new InvalidOperationException("boom"));
//        _sut.AddStage(failingStage.Object);
//        var context = new TestContext();

//        // Act & Assert
//        var ex = await Assert.ThrowsAsync<PipelineStageException>(
//            () => _sut.ExecuteAsync(context, CancellationToken.None));
//        Assert.Equal("Failing", ex.StageName);
//    }

//    [Fact]
//    public async Task ExecuteAsyncWithRetryPolicyShouldInvokePolicy()
//    {
//        // Arrange
//        var stage = CreateExecutingStage("Retried", []).Object;
//        var policy = Policy.NoOpAsync();
//        _sut.AddStage(stage, policy);
//        var context = new TestContext();

//        // Act
//        var result = await _sut.ExecuteAsync(context, CancellationToken.None);

//        // Assert
//        Assert.NotNull(result);
//    }

//    // --- StageNames ---

//    [Fact]
//    public void StageNamesShouldReturnEmptyWhenNoStages()
//    {
//        // Act
//        var names = _sut.StageNames;

//        // Assert
//        Assert.Empty(names);
//    }

//    [Fact]
//    public void StageNamesShouldReturnAllRegisteredNames()
//    {
//        // Arrange
//        _sut.AddStage(CreateMockStage("One").Object);
//        _sut.AddStage(CreateMockStage("Two").Object);
//        _sut.AddStage(CreateMockStage("Three").Object);

//        // Act
//        var names = _sut.StageNames;

//        // Assert
//        Assert.Equal(3, names.Count);
//        Assert.Equal("One", names[0]);
//        Assert.Equal("Two", names[1]);
//        Assert.Equal("Three", names[2]);
//    }

//    // --- Helper Methods ---

//    private static Mock<IPipelineStage<TestContext>> CreateMockStage(string name)
//    {
//        var mock = new Mock<IPipelineStage<TestContext>>();
//        mock.Setup(s => s.Name).Returns(name);
//        mock.Setup(s => s.ShouldRun(It.IsAny<TestContext>())).Returns(true);
//        return mock;
//    }

//    private static Mock<IPipelineStage<TestContext>> CreateExecutingStage(string name, List<string> order)
//    {
//        var mock = new Mock<IPipelineStage<TestContext>>();
//        mock.Setup(s => s.Name).Returns(name);
//        mock.Setup(s => s.ShouldRun(It.IsAny<TestContext>())).Returns(true);
//        mock.Setup(s => s.ExecuteAsync(It.IsAny<TestContext>(), It.IsAny<CancellationToken>()))
//           .Callback<string, CancellationToken>((_, _) => order.Add(name))
//           .Returns(Task.CompletedTask);
//        return mock;
//    }

//    private static Mock<IPipelineStage<TestContext>> CreateNonRunningStage(string name)
//    {
//        var mock = new Mock<IPipelineStage<TestContext>>();
//        mock.Setup(s => s.Name).Returns(name);
//        mock.Setup(s => s.ShouldRun(It.IsAny<TestContext>())).Returns(false);
//        return mock;
//    }

//    private class TestContext
//    {
//    }
//}
