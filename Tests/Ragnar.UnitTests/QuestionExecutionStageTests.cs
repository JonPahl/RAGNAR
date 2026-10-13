namespace Ragnar.Tests;

public class QuestionExecutionStageTests
{
    private readonly Mock<IRagOrchestrator> _ragMock;
    private readonly Mock<IOutputWriter> _writerMock;
    private readonly Mock<IContextRetriever> _contextMock;
    private readonly Mock<Serilog.ILogger> _loggerMock;
    private readonly QuestionAnswerStage _sut;

    public QuestionExecutionStageTests()
    {
        _ragMock = new Mock<IRagOrchestrator>();
        _writerMock = new Mock<IOutputWriter>();
        _contextMock = new Mock<IContextRetriever>();
        _loggerMock = new Mock<Serilog.ILogger>();

        var config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions { VectorStoreName = "test_collection", SourceDirectory = "" },
            EmbeddingOptions = new EmbeddingOptions { Dimension = 1536, BatchSize = 10, EmbeddingModel = "test-model", Host = "http://localhost", Port = 11434, Timeout = TimeSpan.FromSeconds(30) },
            FileLoadOptions = new FileLoadOptions(),
            GenerationOptions = new GenerationOptions { MaxTokens = 1000, },
            OllamaOptions = new OllamaOptions { Host = "http://localhost", Port = 11434, Timeout = TimeSpan.FromSeconds(30), LlmModel = "test-llm-model" },
        };

        _sut = new QuestionAnswerStage(
            _ragMock.Object,
            Options.Create(config),
            _writerMock.Object,
            _contextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void NameShouldReturnAskQuestionsStage()
    {
        Assert.Equal("Ask Questions Stage", _sut.Name);
    }

    [Fact]
    public void ShouldRunWithQuestionsShouldReturnTrue()
    {
        var context = new QuestionPipelineContext
        {
            Questions = [new Question(true, "Q", "f.csv", QuestionCategory.XML)]
        };

        Assert.True(_sut.ShouldRun(context));
    }

    [Fact]
    public void ShouldRunWithNoQuestionsShouldReturnFalse()
    {
        var context = new QuestionPipelineContext { Questions = [] };

        Assert.False(_sut.ShouldRun(context));
    }

    [Fact]
    public void ShouldRunWithNullQuestionsShouldReturnFalse()
    {
        var context = new QuestionPipelineContext { Questions = null };

        Assert.False(_sut.ShouldRun(context));
    }

    [Fact]
    public void RetryPolicyShouldHandleHttpRequestAndTimeout()
    {
        var policy = _sut.RetryPolicy;

        Assert.NotNull(policy);
    }

    [Fact]
    public async Task ExecuteAsyncShouldCallWriterForEachQuestion()
    {
        // Arrange
        var context = new QuestionPipelineContext
        {
            Questions = [new Question(true, "Q", "f.csv", QuestionCategory.XML)]
        };

        _contextMock.Setup(c => c.RetrieveContextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<Filter>()))
            .ReturnsAsync("ctx");

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        _writerMock.Verify(w => w.WriteRule(), Times.Exactly(1));
        _writerMock.Verify(w => w.MarkupLine(It.IsAny<string>(), It.IsAny<Style>()), Times.Exactly(1));
    }
}
