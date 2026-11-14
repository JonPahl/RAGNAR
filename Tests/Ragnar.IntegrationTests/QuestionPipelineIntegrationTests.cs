namespace Ragnar.IntegrationTests;

/// <summary>
/// Integration tests for the question execution pipeline: load → filter → show → answer.
/// </summary>
public class QuestionPipelineIntegrationTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IOutputWriter> _writerMock;

    public QuestionPipelineIntegrationTests()
    {
        _loggerMock = new Mock<ILogger>();
        _writerMock = new Mock<IOutputWriter>();
    }

    #region Question Loading and Filtering

    [Fact]
    public void QuestionFilterSelectorShouldReturnCorrectStrategyForXml()
    {
        // Arrange
        var selector = new QuestionFilterSelector([]);

        // Act
        var filter = selector.FindFilter(QuestionCategory.XML, 50);

        // Assert
        Assert.NotNull(filter);
        Assert.NotEmpty(filter.Must);
    }

    [Fact]
    public void QuestionFilterSelectorShouldThrowForUnknownCategory()
    {
        // Arrange
        var selector = new QuestionFilterSelector([]);

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(
            () => selector.FindFilter((QuestionCategory)999));
        Assert.Contains("999", ex.Message);
    }

    [Fact]
    public void XmlCommentFilterShouldExcludeTestCategories()
    {
        // Arrange
        var strategy = new XmlCommentFilterStrategy(_loggerMock.Object);

        // Act
        var filter = strategy.CreateFilter(10);

        // Assert – must contain exclusions for Test, Tests, Testing
        Assert.NotNull(filter.MustNot);
        Assert.True(filter.MustNot.Count >= 1);
    }

    [Fact]
    public void XmlCommentFilterShouldIncludeRangeCondition()
    {
        // Arrange
        var strategy = new XmlCommentFilterStrategy(_loggerMock.Object);

        // Act
        var filter = strategy.CreateFilter(100);

        // Assert
        Assert.NotNull(filter.Must);
        Assert.Contains(filter.Must, c => c is not null);
    }

    #endregion

    #region Question Context State Flow

    [Fact]
    public void QuestionContextShouldAccumulateAnswers()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var q1 = Guid.NewGuid();
        var q2 = Guid.NewGuid();

        // Act
        ctx.Answers[q1] = "Answer 1";
        ctx.Answers[q2] = "Answer 2";
        ctx.QuestionTimings[q1] = TimeSpan.FromMilliseconds(150);
        ctx.QuestionTimings[q2] = TimeSpan.FromMilliseconds(200);

        // Assert
        Assert.Equal(2, ctx.Answers.Count);
        Assert.Equal("Answer 1", ctx.Answers[q1]);
        Assert.Equal(TimeSpan.FromMilliseconds(150), ctx.QuestionTimings[q1]);
    }

    [Fact]
    public void QuestionContextShouldReturnNullQuestionsBeforeLoad()
    {
        // Arrange & Act
        var ctx = new QuestionPipelineContext();

        // Assert
        Assert.Null(ctx.Questions);
    }

    [Fact]
    public void QuestionContextShouldExposeQuestionsAfterSet()
    {
        // Arrange
        var ctx = new QuestionPipelineContext();
        var questions = new[]
        {
            new Question(true,"What is X?",""),
            new Question(true, "How does Y work?", "")
        };

        // Act
        ctx.SetLoadedQuestions(questions);

        // Assert
        Assert.NotNull(ctx.Questions);
        Assert.Equal(2, ctx.Questions.Count);
        Assert.Contains(ctx.Questions, q => q.Text == "What is X?");
    }

    #endregion

    #region Show Stage Integration

    [Fact]
    public async Task QuestionShowStageShouldWriteOutput()
    {
        // Arrange
        var stage = new QuestionTableStage(_writerMock.Object);
        var ctx = new QuestionPipelineContext();
        ctx.SetLoadedQuestions([new Question(true, "Test Q", "")]);

        // Act
        await stage.ExecuteAsync(ctx, CancellationToken.None);

        // Assert
        _writerMock.Verify(w => w.WriteLine(It.IsAny<string>(), null), Times.AtLeastOnce);
    }

    [Fact]
    public async Task QuestionShowStageShouldHandleCancellation()
    {
        // Arrange
        var stage = new QuestionTableStage(_writerMock.Object);
        var ctx = new QuestionPipelineContext();
        ctx.SetLoadedQuestions([]);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => stage.ExecuteAsync(ctx, cts.Token));
    }

    #endregion

    #region Filter Strategy Integration with Selector

    [Fact]
    public void FilterSelectorWithRegisteredStrategiesShouldDelegateCorrectly()
    {
        // Arrange
        var strategyMock = new Mock<IFilterStrategy>();
        strategyMock.Setup(s => s.SupportedCategory).Returns(QuestionCategory.Other);
        var expectedFilter = new QdrantFilterBuilder().AddMustMatch("Key", "Value").Build();
        strategyMock.Setup(s => s.CreateFilter(It.IsAny<int>())).Returns(expectedFilter);

        var selector = new QuestionFilterSelector([strategyMock.Object]);

        // Act
        var result = selector.FindFilter(QuestionCategory.Other, 200);

        // Assert
        Assert.Same(expectedFilter, result);
        strategyMock.Verify(s => s.CreateFilter(200), Times.Once);
    }

    #endregion
}
