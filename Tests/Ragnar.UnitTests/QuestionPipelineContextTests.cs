namespace Ragnar.Tests;

public class QuestionPipelineContextTests
{
    [Fact]
    public void QuestionsShouldBeNullBeforeSet()
    {
        var ctx = new QuestionPipelineContext();

        Assert.Null(ctx.Questions);
    }

    [Fact]
    public void SetQuestionsWithEmptyListShouldReturnEmpty()
    {
        var ctx = new QuestionPipelineContext();

        ctx.SetQuestions([]);

        Assert.NotNull(ctx.Questions);
        Assert.Empty(ctx.Questions);
    }

    [Fact]
    public void ResponsesShouldStartEmpty()
    {
        var ctx = new QuestionPipelineContext();

        Assert.Empty(ctx.Responses);
    }

    [Fact]
    public void ResponsesShouldSupportAddAndRetrieve()
    {
        var ctx = new QuestionPipelineContext();
        var qid = Guid.NewGuid();

        ctx.Responses[qid] = "Hello";

        Assert.True(ctx.Responses.ContainsKey(qid));
        Assert.Equal("Hello", ctx.Responses[qid]);
    }

    [Fact]
    public void TimingsShouldStartEmpty()
    {
        var ctx = new QuestionPipelineContext();

        Assert.Empty(ctx.Timings);
    }

    [Fact]
    public void TimingsShouldSupportAddAndRetrieve()
    {
        var ctx = new QuestionPipelineContext();
        var qid = Guid.NewGuid();
        var duration = TimeSpan.FromSeconds(1.5);

        ctx.Timings[qid] = duration;

        Assert.True(ctx.Timings.ContainsKey(qid));
        Assert.Equal(duration, ctx.Timings[qid]);
    }
}
