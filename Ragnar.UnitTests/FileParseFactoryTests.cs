namespace Ragnar.Tests;

public class FileParseFactoryTests
{
    private readonly Mock<Serilog.ILogger> _loggerMock = new();

    private readonly Mock<IOptions<RagnarConfig>> _configMock = new();

    [Fact]
    public async Task ParseAsync_DelegatesToCodeParser_ForCsFiles()
    {
        var factory = new FileParseFactory(_configMock.Object, _loggerMock.Object);

        // Create temp .cs file
        var csFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".cs");
        await File.WriteAllTextAsync(csFile, "class Test {}", CancellationToken.None);

        try
        {
            var result = await factory.ParseAsync(csFile, CancellationToken.None);
            result.Should().NotBeNull();
        }
        finally
        {
            if (File.Exists(csFile)) File.Delete(csFile);
        }
    }
    [Fact]
    public async Task ParseAsync_Throws_For_Unsupported_Extension()
    {
        var factory = new FileParseFactory(_configMock.Object, _loggerMock.Object);
        var jsonFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");
        await File.WriteAllTextAsync(jsonFile, "{}", CancellationToken.None);
        try
        {
            await Assert.ThrowsAsync<NullReferenceException>(() => factory.ParseAsync(jsonFile, CancellationToken.None));
        }
        finally
        {
            if (File.Exists(jsonFile)) File.Delete(jsonFile);
        }
    }
}
