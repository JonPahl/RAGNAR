namespace Ragnar.Tests;

public class QuestionPipelineContextTests
{
    [Fact]
    public void QuestionsShouldBeNullBeforeSet()
    {
        // Arrange & Act
        var ctx = new QuestionPipelineContext();

        // Assert
        Assert.Null(ctx.Questions);
    }

    [Fact]
    public void SetQuestionsShouldStoreList()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var questions = new List<Question> { new(true, "Q1", ""), new(true, "Q2", "") };

        // Act
        ctx.SetQuestions(questions);

        // Assert
        Assert.NotNull(ctx.Questions);
        Assert.Equal(2, ctx.Questions.Count);
        Assert.Contains(questions[0], ctx.Questions);
    }

    [Fact]
    public void SetQuestionsShouldOverwritePreviousValue()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        ctx.SetQuestions([new(true, "old", "")]);

        // Act
        ctx.SetQuestions([new(true, "new", ""), new(true, "new2", "")]);

        // Assert
        Assert.Equal(2, ctx.Questions.Count);
    }

    [Fact]
    public void ResponsesShouldBeEmptyByDefault()
    {
        // Arrange & Act
        var ctx = new QuestionPipelineContext();

        // Assert
        Assert.Empty(ctx.Responses);
    }

    [Fact]
    public void ResponsesShouldSupportConcurrentWrites()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var tasks = Enumerable.Range(0, 100)
            .Select(i => Task.Run(() => ctx.Responses.TryAdd(Guid.NewGuid(), $"Response {i}")));

        // Act
        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.Equal(100, ctx.Responses.Count);
    }

    [Fact]
    public void TimingsShouldBeEmptyByDefault()
    {
        // Arrange & Act
        var ctx = new QuestionPipelineContext();

        // Assert
        Assert.Empty(ctx.Timings);
    }

    [Fact]
    public void TimingsShouldStoreValues()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var id = Guid.NewGuid();

        // Act
        ctx.Timings.TryAdd(id, TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(ctx.Timings.ContainsKey(id));
        Assert.Equal(TimeSpan.FromSeconds(5), ctx.Timings[id]);
    }

    [Fact]
    public void ResponsesAndTimingsShouldBeThreadSafe()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var tasks = new List<Task>();

        for (var i = 0; i < 50; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                var id = Guid.NewGuid();
                ctx.Responses.TryAdd(id, $"R{i}");
                ctx.Timings.TryAdd(id, TimeSpan.FromMilliseconds(i));
            }, cts.Token));
        }

        // Act
        Task.WaitAll([.. tasks], cts.Token);

        // Assert
        Assert.Equal(50, ctx.Responses.Count);
        Assert.Equal(50, ctx.Timings.Count);
    }
}
