namespace Ragnar.Tests;

public class EmbeddingOptionsTests
{
    [Fact]
    public void EmbeddingOptionsShouldStoreAllProperties()
    {
        var opts = new EmbeddingOptions
        {
            Host = "localhost",
            Port = 8080,
            EmbeddingModel = "nomic-embed-text",
            Timeout = TimeSpan.FromSeconds(60),
            Dimension = 768,
            BatchSize = 32
        };

        Assert.Equal("localhost", opts.Host);
        Assert.Equal(8080, opts.Port);
        Assert.Equal("nomic-embed-text", opts.EmbeddingModel);
        Assert.Equal(TimeSpan.FromSeconds(60), opts.Timeout);
        Assert.Equal(768UL, opts.Dimension);
        Assert.Equal(32, opts.BatchSize);
    }

    [Fact]
    public void EmbeddingOptionsShouldSupportEquality()
    {
        var a = new EmbeddingOptions
        {
            Host = "h",
            Port = 1,
            EmbeddingModel = "m",
            Timeout = TimeSpan.FromSeconds(10),
            Dimension = 128,
            BatchSize = 4
        };
        var b = new EmbeddingOptions
        {
            Host = "h",
            Port = 1,
            EmbeddingModel = "m",
            Timeout = TimeSpan.FromSeconds(10),
            Dimension = 128,
            BatchSize = 4
        };

        Assert.Equal(a, b);
    }
}
