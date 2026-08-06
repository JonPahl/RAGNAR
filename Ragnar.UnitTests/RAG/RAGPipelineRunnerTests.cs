namespace Ragnar.UnitTests.RAG;

public class RagPipelineRunnerTests
{
    private readonly Mock<IOutputWriter> _mockWriter;
    private readonly Mock<IEmbeddingPipeline> _mockEmbeddingPipeline;
    private readonly Mock<IKnowledgeBaseInitialize> _mockRagPipeline;
    private readonly Mock<IApplicationBanner> _mockBranding;
    private readonly Mock<ISummaryService> _mockSummaryService;
    private readonly RagPipelineRunner _runner;

    public RagPipelineRunnerTests()
    {
        _mockWriter = new Mock<IOutputWriter>();
        _mockEmbeddingPipeline = new Mock<IEmbeddingPipeline>();
        _mockRagPipeline = new Mock<IKnowledgeBaseInitialize>();
        _mockBranding = new Mock<IApplicationBanner>();
        _mockSummaryService = new Mock<ISummaryService>();

        _runner = new RagPipelineRunner(
            _mockWriter.Object,
            _mockEmbeddingPipeline.Object,
            _mockRagPipeline.Object,
            _mockBranding.Object,
            _mockSummaryService.Object);
    }

    [Fact]
    public async Task StartAsyncExecutesPipelineStepsInOrder()
    {
        // Arrange
        var ct = CancellationToken.None;

        // Act
        await _runner.StartAsync(ct);

        // Assert
        _mockBranding.Verify(b => b.RenderBranding(), Times.Once);
        _mockEmbeddingPipeline.Verify(e => e.EnsureCollectionExistsAsync(ct), Times.Once);
        _mockEmbeddingPipeline.Verify(e => e.PopulateAsync(ct), Times.Once);
        _mockRagPipeline.Verify(r => r.AskQuestionsAsync(ct), Times.Once);
        _mockSummaryService.Verify(s => s.SummarizeAllResponsesAsync(ct), Times.Once);
    }
}
