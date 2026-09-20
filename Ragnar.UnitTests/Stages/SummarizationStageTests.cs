namespace Ragnar.Tests.Stages;

public class SummarizationStageTests
{
    private readonly Mock<ISummaryService> _summaryServiceMock;
    private readonly Mock<IOutputWriter> _writerMock;
    private readonly EmbeddingContext _context;
    private readonly SummarizationStage _sut;

    public SummarizationStageTests()
    {
        _summaryServiceMock = new Mock<ISummaryService>();
        _writerMock = new Mock<IOutputWriter>();
        _context = new EmbeddingContext();
        _sut = new SummarizationStage(_summaryServiceMock.Object, _writerMock.Object);
    }

    [Fact]
    public void NameShouldReturnExpectedStageName()
    {
        Assert.Equal("Summarize Results Stage", _sut.Name);
    }

    [Fact]
    public async Task ExecuteAsyncWhenSummaryServiceThrowsShouldPropagateException()
    {
        // Arrange
        var ct = CancellationToken.None;
        _summaryServiceMock
            .Setup(s => s.SummarizeAllResponsesAsync(ct))
            .ThrowsAsync(new InvalidOperationException("Summary failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecuteAsync(_context, ct));
    }

    [Fact]
    public async Task ExecuteAsyncWhenCancelledShouldNotCallWriterAfterService()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _summaryServiceMock
            .Setup(s => s.SummarizeAllResponsesAsync(cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => _sut.ExecuteAsync(_context, cts.Token));
        _writerMock.Verify(w => w.WriteRule(), Times.Never);

        cts.Dispose();
    }
}
