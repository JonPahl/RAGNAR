using Moq;

using Ragnar.Branding;
using Ragnar.Core.ConsoleWriter;
using Ragnar.Core.Interface;

using RAGNAR.OutputResponse;

namespace Ragnar.IntegrationTests;

public class RagPipelineRunnerTests
{
    [Fact]
    public async Task StartAsync_Should_Run_Full_Pipeline()
    {
        // Arrange
        var mockWriter = new Mock<IOutputWriter>();
        var mockEmbeddingPipeline = new Mock<IEmbeddingPipeline>();
        var mockRagPipeline = new Mock<IKnowledgeBaseInitialize>();
        var mockBranding = new Mock<IApplicationBanner>();
        var mockSummaryService = new Mock<ISummaryService>();

        var runner = new RagPipelineRunner(
            mockWriter.Object,
            mockEmbeddingPipeline.Object,
            mockRagPipeline.Object,
            mockBranding.Object,
            mockSummaryService.Object
        );

        // Act
        await runner.StartAsync(CancellationToken.None);

        // Assert
        mockBranding.Verify(b => b.RenderBranding(), Times.Once);
        mockEmbeddingPipeline.Verify(p => p.EnsureCollectionExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockEmbeddingPipeline.Verify(p => p.PopulateAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockRagPipeline.Verify(p => p.AskQuestionsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockSummaryService.Verify(s => s.SummarizeAllResponsesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
