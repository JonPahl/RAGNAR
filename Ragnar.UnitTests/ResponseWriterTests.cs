namespace Ragnar.UnitTests;

public class ResponseWriterTests
{
    private readonly Mock<IOptions<AppConfiguration>> MockConfig;
    private readonly string TempDir;

    public ResponseWriterTests()
    {
        TempDir = Path.Combine(Path.GetTempPath(), $"RagTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(TempDir);

        var AppOptions = new RagOptions { SourceDirectory = TempDir, VectorStoreName = "", SaveDirectory = "" };

        MockConfig = new Mock<IOptions<AppConfiguration>>();
        MockConfig.Setup(X => X.Value.RagOptions).Returns(AppOptions);
    }
}
