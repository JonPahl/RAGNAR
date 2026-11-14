namespace Ragnar.Tests.Options;

public class EmbeddingOptionsTests
{
    [Fact]
    public void ShouldStoreAllProperties()
    {
        // Arrange
        var host = new Uri("http://localhost:11434");
        var timeout = TimeSpan.FromSeconds(60);

        // Act
        var opts = new EmbeddingOptions
        {
            Host = host,
            EmbeddingModel = "nomic-embed-text",
            Timeout = timeout,
            Dimension = 768,
            BatchSize = 32
        };

        // Assert
        Assert.Equal(host, opts.Host);
        Assert.Equal("nomic-embed-text", opts.EmbeddingModel);
        Assert.Equal(timeout, opts.Timeout);
        Assert.Equal(768UL, opts.Dimension);
        Assert.Equal(32, opts.BatchSize);
    }

    [Fact]
    public void ShouldSupportEquality()
    {
        // Arrange
        var host = new Uri("http://localhost");
        var a = new EmbeddingOptions { Host = host, EmbeddingModel = "model", Timeout = TimeSpan.FromSeconds(30), Dimension = 512, BatchSize = 16 };
        var b = new EmbeddingOptions { Host = host, EmbeddingModel = "model", Timeout = TimeSpan.FromSeconds(30), Dimension = 512, BatchSize = 16 };

        // Assert
        Assert.Equal(a, b);
    }

    [Fact]
    public void ShouldDifferWhenDimensionDiffers()
    {
        // Arrange
        var host = new Uri("http://localhost");
        var a = new EmbeddingOptions { Host = host, EmbeddingModel = "m", Timeout = TimeSpan.FromSeconds(30), Dimension = 512, BatchSize = 16 };
        var b = new EmbeddingOptions { Host = host, EmbeddingModel = "m", Timeout = TimeSpan.FromSeconds(30), Dimension = 1024, BatchSize = 16 };

        // Assert
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ShouldBeImmutableViaWithExpression()
    {
        // Arrange
        var host = new Uri("http://localhost");
        var original = new EmbeddingOptions { Host = host, EmbeddingModel = "original", Timeout = TimeSpan.FromSeconds(30), Dimension = 768, BatchSize = 16 };

        // Act
        var modified = original with { EmbeddingModel = "changed" };

        // Assert
        Assert.Equal("original", original.EmbeddingModel);
        Assert.Equal("changed", modified.EmbeddingModel);
    }
}
