//namespace Ragnar.UnitTests;

//using Moq;
//using OllamaSharp.Models;
//using Ragnar.Factory;
//using Ragnar.Interfaces;
//using Ragnar.Models;
//using Ragnar.Output;
//using Ragnar.Plugins;
//using Ragnar.RagPipeline;

//public sealed class RagPipelineExecutorTests
//{
//    [Fact]
//    public async Task RunAsync_GeneratesAndSavesResponse()
//    {
//        // Arrange
//        var systemPromptMock = new Mock<ISystemPromptProvider>();
//        systemPromptMock.EmbeddingSetup(p => p.Template).Returns("System prompt");

//        var writerMock = new Mock<IResponseWriter>();
//        writerMock.EmbeddingSetup(w => w.WriteResponseAsync(It.IsAny<SaveDetails>(), It.IsAny<CancellationToken>()))
//                  .ReturnsAsync("path/to/response.md");

//        var ollamaFactoryMock = new Mock<IOllamaClientProvider>();
//        var ollamaProviderMock = new Mock<IOllamaClientProvider>();
//        ollamaProviderMock.EmbeddingSetup(p => p.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
//                          .ReturnsAsync("Generated answer");

//        var pipeline = new RagOrchestrator(writerMock.Object, systemPromptMock.Object, ollamaFactoryMock.Object, ollamaProviderMock.Object);
//        var question = Question.IsEnabled("Test?", "key", QuestionCategory.Refactor);

//        // Act
//        await pipeline.RunAsync(question, "Context", CancellationToken.None);

//        // Assert
//        writerMock.Verify(w => w.WriteResponseAsync(It.IsAny<SaveDetails>(), It.IsAny<CancellationToken>()), Times.Once);
//    }
//}
