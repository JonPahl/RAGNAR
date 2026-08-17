namespace Ragnar.UnitTests;

public class FileParseFactoryTests
{

    private readonly Mock<Serilog.ILogger> LoggerMock = new();

    private readonly Mock<IOptions<ApplicationConfiguration>> ConfigMock = new();

    [Fact]
    public async Task ParseAsync_DelegatesToCodeParser_ForCsFiles()
    {
        var factory = new FileParseFactory(ConfigMock.Object, LoggerMock.Object);

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
            if(File.Exists(csFile)) File.Delete(csFile);
        }
    }
}
