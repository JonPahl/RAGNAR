namespace Ragnar.Tests;

public class GenerationOptionsTests
{
    [Fact]
    public void GenerationOptionsShouldHaveDefaultValues()
    {
        var opts = new GenerationOptions();

        Assert.Equal(32_768, opts.MaxTokens);
        Assert.Equal(65_536, opts.ContextWindow);
        Assert.Equal(8, opts.ThreadCount);
        Assert.Equal(0.2f, opts.Temperature);
        Assert.Equal(1.02f, opts.RepeatPenalty);
    }

    [Fact]
    public void GenerationOptionsShouldAllowCustomValues()
    {
        var opts = new GenerationOptions
        {
            MaxTokens = 4096,
            ContextWindow = 8192,
            ThreadCount = 4,
            Temperature = 0.7f,
            RepeatPenalty = 1.1f
        };

        Assert.Equal(4096, opts.MaxTokens);
        Assert.Equal(8192, opts.ContextWindow);
        Assert.Equal(4, opts.ThreadCount);
        Assert.Equal(0.7f, opts.Temperature);
        Assert.Equal(1.1f, opts.RepeatPenalty);
    }

    [Fact]
    public void GenerationOptionsShouldSupportEquality()
    {
        var a = new GenerationOptions();
        var b = new GenerationOptions();

        Assert.Equal(a, b);
    }

    [Fact]
    public void GenerationOptionsShouldDifferWhenTemperatureDiffers()
    {
        var a = new GenerationOptions { Temperature = 0.1f };
        var b = new GenerationOptions { Temperature = 0.9f };

        Assert.NotEqual(a, b);
    }
}
