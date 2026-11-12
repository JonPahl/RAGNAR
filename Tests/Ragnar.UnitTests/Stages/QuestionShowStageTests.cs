namespace Ragnar.Tests.Stages;

public class QuestionShowStageTests
{
    private readonly QuestionTableStage _sut;

    public QuestionShowStageTests()
    {
        var writer = new Mock<IOutputWriter>();
        _sut = new QuestionTableStage(writer.Object);
    }

    [Fact]
    public void NameShouldReturnShowQuestionsStage()
    {
        Assert.Equal("Show Questions Stage", _sut.Name);
    }

    [Theory]
    [InlineData(typeof(QuestionPipelineContext))] // valid testContext
    [InlineData(typeof(object))]            // null (cast via null!)
    public void ShouldRunShouldAlwaysReturnTrue(Type unused)
    {
        var ctx = unused == typeof(QuestionPipelineContext)
            ? new QuestionPipelineContext()
            : null!;
        Assert.True(_sut.ShouldRun(ctx));
    }

    [Fact]
    public void ShouldRunWithNullContextShouldStillReturnTrue()
    {
        // ShouldRun is defined as always true regardless of testContext
        Assert.True(_sut.ShouldRun(null!));
    }

    [Fact]
    public void RetryPolicyShouldBeNull()
    {
        Assert.Null(_sut.RetryPolicy);
    }

    [Fact]
    public async Task ExecuteAsyncWithCancelledTokenShouldThrow()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var context = new QuestionPipelineContext();
        context.SetLoadedQuestions([]);

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await _sut.ExecuteAsync(context, cts.Token).ConfigureAwait(false));
    }
}
