namespace Ragnar.Tests;

public class GenerationOptionsTests
{
    [Fact]
    public void DefaultValuesShouldMatchDocumentedDefaults()
    {
        var opts = new GenerationOptions();

        Assert.Equal(32_768, opts.MaxTokens);
        Assert.Equal(65_536, opts.ContextWindow);
        Assert.Equal(8, opts.ThreadCount);
        Assert.Equal(0.2f, opts.Temperature);
        Assert.Equal(1.02f, opts.RepeatPenalty);
    }

    [Fact]
    public void MaxTokensShouldAcceptValidRange()
    {
        var opts = new GenerationOptions { MaxTokens = 4096 };
        Assert.Equal(4096, opts.MaxTokens);

        var optsMax = new GenerationOptions { MaxTokens = 131072 };
        Assert.Equal(131072, optsMax.MaxTokens);
    }

    [Fact]
    public void MaxTokensShouldRejectOutOfRange()
    {
        var opts = new GenerationOptions { MaxTokens = 0 };
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(opts);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        Assert.False(System.ComponentModel.DataAnnotations.Validator.TryValidateObject(opts, context, results, true));
    }

    [Fact]
    public void TemperatureShouldAcceptBoundaryValues()
    {
        var low = new GenerationOptions { Temperature = 0.0f };
        Assert.Equal(0.0f, low.Temperature);

        var high = new GenerationOptions { Temperature = 2.0f };
        Assert.Equal(2.0f, high.Temperature);
    }

    [Fact]
    public void RepeatPenaltyShouldAcceptBoundaryValues()
    {
        var low = new GenerationOptions { RepeatPenalty = 1.0f };
        Assert.Equal(1.0f, low.RepeatPenalty);

        var high = new GenerationOptions { RepeatPenalty = 2.0f };
        Assert.Equal(2.0f, high.RepeatPenalty);
    }

    [Fact]
    public void ShouldSupportImmutabilityViaWith()
    {
        var original = new GenerationOptions { MaxTokens = 1024 };
        var copy = original with { MaxTokens = 2048 };

        Assert.Equal(1024, original.MaxTokens);
        Assert.Equal(2048, copy.MaxTokens);
    }

    [Fact]
    public void ShouldSupportEquality()
    {
        var a = new GenerationOptions { MaxTokens = 100, ContextWindow = 200 };
        var b = new GenerationOptions { MaxTokens = 100, ContextWindow = 200 };

        Assert.Equal(a, b);
    }
}
