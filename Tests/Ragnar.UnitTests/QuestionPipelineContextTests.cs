namespace Ragnar.Tests;

public class QuestionPipelineContextTests
{
    [Fact]
    public void QuestionsShouldBeNullBeforeSet()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();

        // Act & Assert
        Assert.Null(ctx.Questions);
    }

    [Fact]
    public void SetQuestionsShouldStoreAndExposeQuestions()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var questions = new[] { new Question(true, "", "") };

        // Act
        ctx.SetLoadedQuestions(questions);

        // Assert
        Assert.NotNull(ctx.Questions);
        Assert.Single(ctx.Questions);
    }

    [Fact]
    public void SetQuestionsWithEmptyListShouldReturnEmptyCollection()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();

        // Act
        ctx.SetLoadedQuestions([]);

        // Assert
        Assert.NotNull(ctx.Questions);
        Assert.Empty(ctx.Questions);
    }

    [Fact]
    public void SetLoadedQuestionsShouldOverwritePreviousValue()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var first = new[] { new Question(true, "", "") };
        var second = new[] { new Question(true, "", ""), new Question(true, "", "") };
        ctx.SetLoadedQuestions(first);

        // Act
        ctx.SetLoadedQuestions(second);

        // Assert
        Assert.Equal(2, ctx.Questions!.Count);
    }

    [Fact]
    public void ResponsesShouldBeEmptyByDefault()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();

        // Act & Assert
        Assert.Empty(ctx.Answers);
    }

    [Fact]
    public void ResponsesShouldStoreAndRetrieveValues()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var qid = Guid.NewGuid();

        // Act
        ctx.Answers[qid] = "hello";

        // Assert
        Assert.Equal("hello", ctx.Answers[qid]);
    }

    [Fact]
    public void TimingsShouldBeEmptyByDefault()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();

        // Act & Assert
        Assert.Empty(ctx.QuestionTimings);
    }

    [Fact]
    public void TimingsShouldStoreAndRetrieveValues()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var qid = Guid.NewGuid();
        var elapsed = TimeSpan.FromMilliseconds(42);

        // Act
        ctx.QuestionTimings[qid] = elapsed;

        // Assert
        Assert.Equal(elapsed, ctx.QuestionTimings[qid]);
    }
}
