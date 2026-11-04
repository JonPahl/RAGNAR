namespace Ragnar.Tests.Options;

public class EmbeddingOptionsTests
{
    [Fact]
    public void ShouldStoreAllProperties()
    {
        var opts = new EmbeddingOptions
        {
            Host = new Uri("http://localhost:11434"),
            EmbeddingModel = "nomic-embed-text",
            Timeout = TimeSpan.FromSeconds(60),
            Dimension = 768,
            BatchSize = 32
        };

        Assert.Equal(new Uri("http://localhost:11434"), opts.Host);
        Assert.Equal("nomic-embed-text", opts.EmbeddingModel);
        Assert.Equal(TimeSpan.FromSeconds(60), opts.Timeout);
        Assert.Equal(768UL, opts.Dimension);
        Assert.Equal(32, opts.BatchSize);
    }

    [Fact]
    public void EmbeddingOptionsShouldStoreAllProperties()
    {
        // Act
        var opts = new EmbeddingOptions
        {
            Host = new Uri("http://localhost:11434"),
            EmbeddingModel = "nomic-embed-text",
            Timeout = TimeSpan.FromSeconds(60),
            Dimension = 768,
            BatchSize = 32
        };

        // Assert
        Assert.Equal(new Uri("http://localhost:11434"), opts.Host);
        Assert.Equal("nomic-embed-text", opts.EmbeddingModel);
        Assert.Equal(TimeSpan.FromSeconds(60), opts.Timeout);
        Assert.Equal((double)768, opts.Dimension);
        Assert.Equal(32, opts.BatchSize);
    }

    [Fact]
    public void DimensionShouldRejectZero()
    {
        // Act
        var opts = new EmbeddingOptions
        {
            Host = new Uri("http://localhost"),
            EmbeddingModel = "model",
            Timeout = TimeSpan.FromSeconds(30),
            Dimension = 0,
            BatchSize = 16
        };

        // Assert
        var ctx = new ValidationContext(opts);
        ICollection<System.ComponentModel.DataAnnotations.ValidationResult>? results = [];
        Assert.False(System.ComponentModel.DataAnnotations.Validator.TryValidateObject(opts, ctx, results, validateAllProperties: true));
    }

    [Fact]
    public void BatchSizeShouldRejectAbove512()
    {
        // Act
        var opts = new EmbeddingOptions
        {
            Host = new Uri("http://localhost"),
            EmbeddingModel = "model",
            Timeout = TimeSpan.FromSeconds(30),
            Dimension = 768,
            BatchSize = 1024
        };

        // Assert
        var ctx = new ValidationContext(opts);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        Assert.False(System.ComponentModel.DataAnnotations.Validator.TryValidateObject(opts, ctx, results, validateAllProperties: true));
    }

    [Fact]
    public void EmbeddingOptionsShouldSupportEquality()
    {
        // Arrange
        var a = new EmbeddingOptions
        {
            Host = new Uri("http://localhost"),
            EmbeddingModel = "model",
            Timeout = TimeSpan.FromSeconds(30),
            Dimension = 512,
            BatchSize = 16
        };
        var b = new EmbeddingOptions
        {
            Host = new Uri("http://localhost"),
            EmbeddingModel = "model",
            Timeout = TimeSpan.FromSeconds(30),
            Dimension = 512,
            BatchSize = 16
        };

        // Assert
        Assert.Equal(a, b);
    }

    [Fact]
    public void EmbeddingOptionsShouldDifferWhenModelDiffers()
    {
        // Arrange
        var a = new EmbeddingOptions
        {
            Host = new Uri("http://localhost"),
            EmbeddingModel = "model-a",
            Timeout = TimeSpan.FromSeconds(30),
            Dimension = 512,
            BatchSize = 16
        };
        var b = new EmbeddingOptions
        {
            Host = new Uri("http://localhost"),
            EmbeddingModel = "model-b",
            Timeout = TimeSpan.FromSeconds(30),
            Dimension = 512,
            BatchSize = 16
        };

        // Assert
        Assert.NotEqual(a, b);
    }
}
