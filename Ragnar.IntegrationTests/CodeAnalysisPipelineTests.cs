//namespace Ragnar.IntegrationTests;

//public class CodeAnalysisPipelineTests
//    : IntegrationSetup
//{

//    [Fact]
//    public async Task ExecuteAsync_Should_GenerateAnswer_WithRetrievedContext ()
//    {
//        // Arrange
//        var writerMock = new Mock<IOutputWriter>();
//        var configWrapper = Options.Create(options);
//        var systemPromptProviderMock = new Mock<ISystemPromptProvider>();
//        systemPromptProviderMock.Setup(p => p.Template).Returns("You are a helpful assistant.");

//        var saveServiceMock = new Mock<IResponseWriter>();
//        saveServiceMock.Setup(r => r.WriteResponseAsync(It.IsAny<SaveDetails>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync("/path/to/response.md");

//        var ollamaClientFactoryMock = new Mock<IOllamaClientFactory>();
//        var ollamaClientMock = new Mock<IOllamaClientFactory>();
//        ollamaClientMock.Setup(c => c.SelectedModel).Returns("qwen2.5-coder:14b");
//        ollamaClientFactoryMock.Setup(f => f.FindClient(OllamaServiceType.Ollama)).Returns(ollamaClientMock.Object);

//        var ollamaResponseMock = new Mock<IOllamaResponse>();
//        ollamaResponseMock.Setup(r => r.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync("Based on the context, here is the answer.");

//        var pipeline = new CodeAnalysisPipeline(
//            writerMock.Object,
//            configWrapper,
//            systemPromptProviderMock.Object,
//            saveServiceMock.Object,
//            ollamaClientFactoryMock.Object,
//            ollamaResponseMock.Object);

//        var question = new Question(isEnabled: true, text: "How to refactor this class?", filename: "Program.cs", category: QuestionCategory.Refactor);
//        var contextText = "public class Program { ... }";

//        // Act
//        await pipeline.ExecuteAsync(question, contextText, CancellationToken.None);

//        // Assert
//        ollamaResponseMock.Verify(r => r.GenerateResponse(
//            It.Is<GenerateRequest>(req =>
//                req.Model == "qwen2.5-coder:14b" &&
//                req.System == "You are a helpful assistant." &&
//                req.Prompt.Contains("Context:") &&
//                req.Prompt.Contains("How to refactor this class?")),
//            It.IsAny<CancellationToken>()), Times.Once);

//        saveServiceMock.Verify(s => s.WriteResponseAsync(
//            It.Is<SaveDetails>(d =>
//                d.Question == question &&
//                d.Response.Contains("Based on the context")),
//            It.IsAny<CancellationToken>()), Times.Once);

//        writerMock.Verify(w => w.MarkupLine(It.IsAny<string>(), It.IsAny<Style>()), Times.AtLeastOnce);
//    }

//    [Fact]
//    public async Task ExecuteAsync_ExcludeOriginalPrompt_WhenConfigFalse ()
//    {
//        // Arrange
//        var configWrapper = Options.Create(options);
//        // ... (same mocks as above, but omit writer.MarkupLine for prompt)

//        var pipeline = new CodeAnalysisPipeline(
//            writerMock.Object, configWrapper, ...);

//        // Act
//        await pipeline.ExecuteAsync(...);

//        // Assert
//        ollamaResponseMock.Verify(...); // same as before
//        writerMock.Verify(w => w.MarkupLine(It.IsAny<string>(), It.IsAny<Style>()), Times.Exactly(1)); // only final summary
//    }
//}
