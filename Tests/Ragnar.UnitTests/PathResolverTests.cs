namespace Ragnar.Tests;

public class PathResolverTests
{
    private readonly Mock<IOptions<RagnarConfig>> _configMock;
    private readonly PathResolver _sut;

    public PathResolverTests()
    {
        var config = new RagnarConfig
        {
            GenerationOptions = new GenerationOptions(),
            ApplicationOptions = new ApplicationOptions
            {
                VectorStoreName = "TestStore",
                SourceDirectory = @"C:\Projects\Ragnar",
                OutputFolder = "Response"
            },
            EmbeddingOptions = new EmbeddingOptions
            {
                Host = "localhost",
                Port = 6334,
                EmbeddingModel = "test",
                Timeout = TimeSpan.FromMinutes(5),
                Dimension = 768,
                BatchSize = 16
            },
            OllamaOptions = new OllamaOptions
            {
                Host = "localhost",
                Port = 11434,
                LlmModel = "test",
                Timeout = TimeSpan.FromMinutes(30)
            },
            FileLoadOptions = new FileLoadOptions
            {
                AllowedExtensions = [".cs"],
                ExcludedFiles = [],
                ExcludedDirectories = []
            }
        };
        _configMock = new Mock<IOptions<RagnarConfig>>();
        _configMock.Setup(o => o.Value).Returns(config);
        _sut = new PathResolver(_configMock.Object);
    }

    [Fact]
    public void ResolveResponseDirectoryWithCategoryShouldIncludeCategoryName()
    {
        // Act
        var result = _sut.ResolveResponseDirectory(QuestionCategory.General);

        // Assert
        Assert.Contains("General", result);
    }
}
