namespace Ragnar.UnitTests;

public class AppConfigurationTests
{

    private AppConfiguration Config { get; init; }

    public AppConfigurationTests()
    {
        Config = new AppConfiguration()
        {
            RagOptions = new()
            { SaveDirectory = "", SourceDirectory = "", VectorStoreName = "" },
            EmbeddingOptions = new()
            {
                Dimension = 0,
                EmbeddingModel = "",
                Host = "",
                Port = 0,
                Timeout = TimeSpan.FromSeconds(30)
            },
            OllamaOptions = new()
            {
                CodeModel = "",
                Host = "",
                Port = 0,
                Timeout = TimeSpan.FromSeconds(45)
            },
            FileLoadOptions = new()
        };
    }

    [Fact]
    public void AppConfiguration_CanBeInstantiated()
    {
        // Assert
        Config.RagOptions.Should().NotBeNull();
        Config.EmbeddingOptions.Should().NotBeNull();
        Config.OllamaOptions.Should().NotBeNull();
        Config.FileLoadOptions.Should().NotBeNull();
    }
}
