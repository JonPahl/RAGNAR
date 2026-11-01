namespace Ragnar.Tests;

public class GenerationOptionsTests
{
    [Fact]
    public void DefaultMaxTokensShouldBe32768()
    {
        var opts = new GenerationOptions();
        Assert.Equal(32_768, opts.MaxTokens);
    }

    [Fact]
    public void DefaultContextWindowShouldBe65536()
    {
        var opts = new GenerationOptions();
        Assert.Equal(65_536, opts.ContextWindow);
    }

    [Fact]
    public void DefaultThreadCountShouldBe8()
    {
        var opts = new GenerationOptions();
        Assert.Equal(8, opts.ThreadCount);
    }

    [Fact]
    public void DefaultTemperatureShouldBe02()
    {
        var opts = new GenerationOptions();
        Assert.Equal(0.2f, opts.Temperature);
    }

    [Fact]
    public void DefaultRepeatPenaltyShouldBe102()
    {
        var opts = new GenerationOptions();
        Assert.Equal(1.02f, opts.RepeatPenalty);
    }

    [Fact]
    public void MaxTokensShouldAcceptValidValues()
    {
        var opts = new GenerationOptions { MaxTokens = 4096 };
        Assert.Equal(4096, opts.MaxTokens);
    }

    [Fact]
    public void GenerationOptionsShouldSupportEquality()
    {
        var a = new GenerationOptions { MaxTokens = 100, Temperature = 0.5f };
        var b = new GenerationOptions { MaxTokens = 100, Temperature = 0.5f };

        Assert.Equal(a, b);
    }

    [Fact]
    public void GenerationOptionsShouldDifferWhenPropertiesDiffer()
    {
        var a = new GenerationOptions { MaxTokens = 100 };
        var b = new GenerationOptions { MaxTokens = 200 };

        Assert.NotEqual(a, b);
    }
}
