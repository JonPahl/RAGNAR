namespace Ragnar.Tests;

public class QuestionPipelineContextTests
{
    [Fact]
    public void QuestionsShouldBeNullBeforeLoad()
    {
        // Act
        var ctx = new QuestionPipelineContext();

        // Assert
        Assert.Null(ctx.Questions);
    }

    [Fact]
    public void SetLoadedQuestionsShouldPopulateQuestionsProperty()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var questions = new List<Question> { new(true, "What is this?", "") };

        // Act
        ctx.SetLoadedQuestions(questions);

        // Assert
        Assert.NotNull(ctx.Questions);
        Assert.Single(ctx.Questions);
        Assert.Equal("What is this?", ctx.Questions[0].Text);
    }

    [Fact]
    public void SetLoadedQuestionsShouldBeReadableAfterSet()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var q1 = new Question(true, "Q1", "");
        var q2 = new Question(true, "Q2", "");
        var questions = new List<Question> { q1, q2 };

        // Act
        ctx.SetLoadedQuestions(questions);

        // Assert
        Assert.Equal(2, ctx.Questions!.Count);
        Assert.Contains(q1, ctx.Questions);
        Assert.Contains(q2, ctx.Questions);
    }

    [Fact]
    public void AnswersShouldStartEmpty()
    {
        // Act
        var ctx = new QuestionPipelineContext();

        // Assert
        Assert.Empty(ctx.Answers);
    }

    [Fact]
    public void AnswersShouldSupportAddAndRetrieve()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var qid = Guid.NewGuid();

        // Act
        ctx.Answers[qid] = "The answer is 42.";

        // Assert
        Assert.True(ctx.Answers.ContainsKey(qid));
        Assert.Equal("The answer is 42.", ctx.Answers[qid]);
    }

    [Fact]
    public void AnswersShouldSupportMultipleEntries()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var qid1 = Guid.NewGuid();
        var qid2 = Guid.NewGuid();

        // Act
        ctx.Answers[qid1] = "Answer 1";
        ctx.Answers[qid2] = "Answer 2";

        // Assert
        Assert.Equal(2, ctx.Answers.Count);
        Assert.Equal("Answer 1", ctx.Answers[qid1]);
        Assert.Equal("Answer 2", ctx.Answers[qid2]);
    }

    [Fact]
    public void QuestionTimingsShouldStartEmpty()
    {
        // Act
        var ctx = new QuestionPipelineContext();

        // Assert
        Assert.Empty(ctx.QuestionTimings);
    }

    [Fact]
    public void QuestionTimingsShouldSupportAddAndRetrieve()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var qid = Guid.NewGuid();
        var elapsed = TimeSpan.FromSeconds(1.5);

        // Act
        ctx.QuestionTimings[qid] = elapsed;

        // Assert
        Assert.True(ctx.QuestionTimings.ContainsKey(qid));
        Assert.Equal(elapsed, ctx.QuestionTimings[qid]);
    }

    [Fact]
    public void QuestionTimingsShouldSupportOverwrite()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var qid = Guid.NewGuid();

        // Act
        ctx.QuestionTimings[qid] = TimeSpan.FromSeconds(1);
        ctx.QuestionTimings[qid] = TimeSpan.FromSeconds(5);

        // Assert
        Assert.Single(ctx.QuestionTimings);
        Assert.Equal(TimeSpan.FromSeconds(5), ctx.QuestionTimings[qid]);
    }
}

