namespace Ragnar.Tests;

public class QuestionPipelineContextTests
{
    [Fact]
    public void QuestionsShouldDefaultToEmptyList()
    {
        // Arrange & Act
        var ctx = new QuestionPipelineContext();

        // Assert
        Assert.NotNull(ctx.Questions);
        Assert.Empty(ctx.Questions);
    }

    [Fact]
    public void QuestionsShouldBeSettable()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var questions = new List<Question>
        {
            new(true, "Q1","", QuestionCategory.XML),
            new(true, "Q2","", QuestionCategory.XML)
        };

        // Act
        ctx.Questions = questions;

        // Assert
        Assert.Equal(2, ctx.Questions.Count);
    }

    [Fact]
    public void ResponsesShouldDefaultToEmptyConcurrentDictionary()
    {
        // Arrange & Act
        var ctx = new QuestionPipelineContext();

        // Assert
        Assert.NotNull(ctx.Responses);
        Assert.Empty(ctx.Responses);
    }

    [Fact]
    public void ResponsesShouldSupportAddAndGet()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var id = Guid.NewGuid();

        // Act
        ctx.Responses[id] = "Answer text";

        // Assert
        Assert.True(ctx.Responses.ContainsKey(id));
        Assert.Equal("Answer text", ctx.Responses[id]);
    }

    [Fact]
    public void TimingsShouldDefaultToEmptyConcurrentDictionary()
    {
        // Arrange & Act
        var ctx = new QuestionPipelineContext();

        // Assert
        Assert.NotNull(ctx.Timings);
        Assert.Empty(ctx.Timings);
    }

    [Fact]
    public void TimingsShouldSupportAddAndGet()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var id = Guid.NewGuid();
        var duration = TimeSpan.FromMilliseconds(42);

        // Act
        ctx.Timings[id] = duration;

        // Assert
        Assert.True(ctx.Timings.ContainsKey(id));
        Assert.Equal(duration, ctx.Timings[id]);
    }

    [Fact]
    public void ContextShouldSupportMultipleQuestionsAndResponses()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Act
        ctx.Questions = [
            new Question(true, "A","", QuestionCategory.Other),
            new Question(true, "B","", QuestionCategory.Other)
        ];
        ctx.Responses[id1] = "resp1";
        ctx.Responses[id2] = "resp2";
        ctx.Timings[id1] = TimeSpan.FromSeconds(1);
        ctx.Timings[id2] = TimeSpan.FromSeconds(2);

        // Assert
        Assert.Equal(2, ctx.Questions.Count);
        Assert.Equal(2, ctx.Responses.Count);
        Assert.Equal(2, ctx.Timings.Count);
    }
}
