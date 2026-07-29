//namespace Ragnar.IntegrationTests;

//public class RagPipelineRunnerTests
//{
//    [Fact]
//    public async Task StartAsyncShouldRunFullPipeline ()
//    {
//        // Arrange
//        var writerMock = new Mock<IOutputWriter>();
//        var embeddingPipelineMock = new Mock<IEmbeddingPipeline>();
//        var ragPipelineMock = new Mock<IKnowledgeBaseInitialize>();
//        var bannerMock = new Mock<IApplicationBanner>();
//        var summaryServiceMock = new Mock<ISummaryService>();

//        var runner = new RagPipelineRunner(
//            writerMock.Object,
//            embeddingPipelineMock.Object,
//            ragPipelineMock.Object,
//            bannerMock.Object,
//            summaryServiceMock.Object);

//        // Act
//        await runner.StartAsync(CancellationToken.None);

//        // Assert
//        bannerMock.Verify(b => b.RenderBranding(), Times.Once);
//        embeddingPipelineMock.Verify(p => p.EnsureCollectionExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
//        embeddingPipelineMock.Verify(p => p.PopulateAsync(It.IsAny<CancellationToken>()), Times.Once);
//        ragPipelineMock.Verify(p => p.AskQuestionsAsync(It.IsAny<CancellationToken>()), Times.Once);
//        summaryServiceMock.Verify(s => s.SummarizeAllResponsesAsync(It.IsAny<CancellationToken>()), Times.Once);
//        writerMock.Verify(w => w.MarkupLine(It.IsAny<string>(), It.IsAny<Style>()), Times.AtLeastOnce);
//        writerMock.Verify(w => w.WriteLine(), Times.Exactly(2)); // "RAG pipeline completed." + final rule
//        writerMock.Verify(w => w.WriteRule(), Times.Exactly(2));
//    }
//}
