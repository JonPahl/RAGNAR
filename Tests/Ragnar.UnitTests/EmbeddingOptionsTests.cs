namespace Ragnar.Tests;

public class EmbeddingOptionsTests
{
    [Fact]
    public void ShouldStoreAllProperties()
    {
        // Arrange & Act
        var opts = new EmbeddingOptions
        {
            Host = "localhost",
            Port = 8080,
            EmbeddingModel = "nomic-embed-text",
            Timeout = TimeSpan.FromSeconds(60),
            Dimension = 768,
            BatchSize = 32
        };

        // Assert
        Assert.Equal("localhost", opts.Host);
        Assert.Equal(8080, opts.Port);
        Assert.Equal("nomic-embed-text", opts.EmbeddingModel);
        Assert.Equal(TimeSpan.FromSeconds(60), opts.Timeout);
        Assert.Equal(768UL, opts.Dimension);
        Assert.Equal(32, opts.BatchSize);
    }

    [Fact]
    public void ShouldSupportImmutability()
    {
        // Arrange
        var original = new EmbeddingOptions
        {
            Host = "localhost",
            Port = 8080,
            EmbeddingModel = "model-a",
            Timeout = TimeSpan.FromSeconds(30),
            Dimension = 768,
            BatchSize = 16
        };

        // Act
        var modified = original with { Port = 9090 };

        // Assert
        Assert.Equal(8080, original.Port);
        Assert.Equal(9090, modified.Port);
    }

    [Fact]
    public void EqualityShouldWorkWhenAllPropertiesMatch()
    {
        // Arrange
        var a = new EmbeddingOptions { Host = "h", Port = 1, EmbeddingModel = "m", Timeout = TimeSpan.Zero, Dimension = 1, BatchSize = 1 };
        var b = new EmbeddingOptions { Host = "h", Port = 1, EmbeddingModel = "m", Timeout = TimeSpan.Zero, Dimension = 1, BatchSize = 1 };

        // Act & Assert
        Assert.Equal(a, b);
    }

    [Fact]
    public void InequalityShouldWorkWhenDimensionDiffers()
    {
        // Arrange
        var a = new EmbeddingOptions { Host = "h", Port = 1, EmbeddingModel = "m", Timeout = TimeSpan.Zero, Dimension = 768, BatchSize = 16 };
        var b = new EmbeddingOptions { Host = "h", Port = 1, EmbeddingModel = "m", Timeout = TimeSpan.Zero, Dimension = 1536, BatchSize = 16 };

        // Act & Assert
        Assert.NotEqual(a, b);
    }
}
