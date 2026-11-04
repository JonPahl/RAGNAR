namespace Ragnar.Tests;

public class CodeParserFactoryTests
{
    private readonly Mock<Serilog.ILogger> _loggerMock;
    private readonly Mock<IOptions<RagnarConfig>> _optionsMock;
    private readonly Mock<ISyntaxTreeChunker> _chunkerMock;

    public CodeParserFactoryTests()
    {
        _loggerMock = new Mock<Serilog.ILogger>();
        _optionsMock = new Mock<IOptions<RagnarConfig>>();
        _chunkerMock = new Mock<ISyntaxTreeChunker>();
    }

    [Fact]
    public async Task ParseAsyncShouldThrowIfTokenAlreadyCancelled()
    {
        var factory = new CodeParserFactory(_loggerMock.Object, _optionsMock.Object, _chunkerMock.Object);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => factory.ParseAsync("test.cs", cts.Token));
    }

    [Fact]
    public void ConstructorShouldNotBeNull()
    {
        var factory = new CodeParserFactory(_loggerMock.Object, _optionsMock.Object, _chunkerMock.Object);
        Assert.NotNull(factory);
    }
}
