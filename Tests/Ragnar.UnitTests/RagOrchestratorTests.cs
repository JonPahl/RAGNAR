namespace Ragnar.Tests;

public sealed class RagOrchestratorTests
{
    private readonly Mock<ILogger<RagOrchestrator>> _loggerMock = new();
    private readonly Mock<IOutputWriter> _writerMock = new();
    private readonly Mock<IOptions<RagnarConfig>> _configMock = new();
    private readonly Mock<IChatPromptProvider> _promptMock = new();
    private readonly Mock<IRagPromptBuilder> _builderMock = new();
    private readonly Mock<IResponseWriter> _saveMock = new();
    private readonly Mock<IOllamaClientFactory> _factoryMock = new();
    private readonly Mock<IOllamaGenerationService> _genMock = new();
    private readonly RagOrchestrator _sut;

    public RagOrchestratorTests()
    {
        _configMock.Setup(x => x.Value.ApplicationOptions.IncludeOriginalPrompt).Returns(true);
        _configMock.Setup(x => x.Value.GenerationOptions.Temperature).Returns(0.7f);

        _sut = new(_loggerMock.Object, _writerMock.Object, _configMock.Object,
                   Mock.Of<IChatPromptProvider>(), // Keyed service workaround for test setup
                   _builderMock.Object, _saveMock.Object, _factoryMock.Object, _genMock.Object);
    }

    [Fact]
    public async Task ExecuteAsyncShouldBuildPromptAndGenerateResponse()
    {
        // Arrange
        var q = new Question(true, "Test Q", "test.cs", QuestionCategory.Other);
        var contextText = "Relevant code snippet";

        _builderMock.Setup(x => x.WithQuestion(q)).Returns(_builderMock.Object);
        _builderMock.Setup(x => x.WithContext(contextText)).Returns(_builderMock.Object);
        _builderMock.Setup(x => x.Build()).Returns("Final Prompt");
        _genMock.Setup(x => x.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("LLM Response");

        // Act
        await _sut.ExecuteAsync(q, contextText, CancellationToken.None);

        // Assert
        _builderMock.Verify(x => x.Build(), Times.Once);
        _genMock.Verify(x => x.GenerateResponse(It.Is<GenerateRequest>(r => r.Prompt == "Final Prompt"), It.IsAny<CancellationToken>()), Times.Once);
        _saveMock.Verify(x => x.WriteResponseAsync(It.IsAny<ResponseRecord>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncShouldAppendOriginalPromptWhenConfigured()
    {
        // Arrange
        var q = new Question(true, "Test Q", "test.cs", QuestionCategory.Other);
        _genMock.Setup(x => x.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Response");

        // Act
        await _sut.ExecuteAsync(q, "", CancellationToken.None);

        // Assert
        var record = _saveMock.Invocations[0].Arguments[0] as ResponseRecord;
        Assert.NotNull(record);
        Assert.Contains("[Original Prompt]", record.Answer, StringComparison.InvariantCultureIgnoreCase);
    }
}
