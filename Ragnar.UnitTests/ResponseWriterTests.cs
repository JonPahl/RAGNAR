namespace Ragnar.UnitTests;

public class ResponseWriterTests
{
    private readonly Mock<IOptions<AppConfiguration>> _mockConfig;
    private readonly ResponseWriter _writer;
    private readonly string _tempDir;

    public ResponseWriterTests ()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"RagTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        var appOptions = new RagOptions { SourceDirectory = _tempDir, VectorStoreName = "", SaveDirectory = "" };
        _mockConfig = new Mock<IOptions<AppConfiguration>>();
        _mockConfig.Setup(x => x.Value.RagOptions).Returns(appOptions);

        _writer = new ResponseWriter(_mockConfig.Object);
    }
}
