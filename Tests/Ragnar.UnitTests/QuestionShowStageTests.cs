namespace Ragnar.Tests;


public class QuestionShowStageTests
{
    private readonly QuestionShowStage _sut;

    public QuestionShowStageTests()
    {
        _sut = new QuestionShowStage();
    }

    [Fact]
    public void NameShouldReturnShowQuestionsStage()
    {
        Assert.Equal("Show Questions Stage", _sut.Name);
    }

    [Fact]
    public void ShouldRunShouldAlwaysReturnTrue()
    {
        var context = new QuestionPipelineContext();

        Assert.True(_sut.ShouldRun(context));
    }

    [Fact]
    public void ShouldRunWithNullContextShouldStillReturnTrue()
    {
        // ShouldRun is defined as always true regardless of context
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
        var context = new QuestionPipelineContext { Questions = [] };

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await _sut.ExecuteAsync(context, cts.Token).ConfigureAwait(false));
    }
}
