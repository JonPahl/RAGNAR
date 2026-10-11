namespace Ragnar.Tests;

public class QuestionPipelineContextTests
{
    [Fact]
    public void QuestionsShouldDefaultToEmptyList()
    {
        var ctx = new QuestionPipelineContext();
        Assert.NotNull(ctx.Questions);
        Assert.Empty(ctx.Questions);
    }

    [Fact]
    public void ResponsesShouldDefaultToEmptyDictionary()
    {
        var ctx = new QuestionPipelineContext();
        Assert.NotNull(ctx.Responses);
        Assert.Empty(ctx.Responses);
    }

    [Fact]
    public void TimingsShouldDefaultToEmptyDictionary()
    {
        var ctx = new QuestionPipelineContext();
        Assert.NotNull(ctx.Timings);
        Assert.Empty(ctx.Timings);
    }

    [Fact]
    public void ResponsesShouldAcceptNewEntries()
    {
        var ctx = new QuestionPipelineContext();
        var qid = Guid.NewGuid();
        ctx.Responses[qid] = "Answer text";

        Assert.True(ctx.Responses.ContainsKey(qid));
        Assert.Equal("Answer text", ctx.Responses[qid]);
    }

    [Fact]
    public void TimingsShouldAcceptNewEntries()
    {
        var ctx = new QuestionPipelineContext();
        var qid = Guid.NewGuid();
        ctx.Timings[qid] = TimeSpan.FromSeconds(1.5);

        Assert.True(ctx.Timings.ContainsKey(qid));
        Assert.Equal(TimeSpan.FromSeconds(1.5), ctx.Timings[qid]);
    }

    [Fact]
    public void QuestionsShouldBeSettable()
    {
        var ctx = new QuestionPipelineContext();
        var questions = new[]
        {
            new Question(true, "What is C#?","",QuestionCategory.Other)
        };
        ctx.Questions = questions;

        Assert.Single(ctx.Questions);
        Assert.Equal("What is C#?", ctx.Questions[0].Text);
    }
}
