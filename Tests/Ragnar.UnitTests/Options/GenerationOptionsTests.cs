namespace Ragnar.Tests.Options;

public class GenerationOptionsTests
{
    [Fact]
    public void MaxTokensShouldDefaultTo32768()
    {
        // Act
        var opts = new GenerationOptions();

        // Assert
        Assert.Equal(32_768, opts.MaxTokens);
    }

    [Fact]
    public void ContextWindowShouldDefaultTo65536()
    {
        // Act
        var opts = new GenerationOptions();

        // Assert
        Assert.Equal(65_536, opts.ContextWindow);
    }

    [Fact]
    public void ThreadCountShouldDefaultTo8()
    {
        // Act
        var opts = new GenerationOptions();

        // Assert
        Assert.Equal(8, opts.ThreadCount);
    }

    [Fact]
    public void TemperatureShouldDefaultTo02()
    {
        // Act
        var opts = new GenerationOptions();

        // Assert
        Assert.Equal(0.2f, opts.Temperature, precision: 3);
    }

    [Fact]
    public void RepeatPenaltyShouldDefaultTo102()
    {
        // Act
        var opts = new GenerationOptions();

        // Assert
        Assert.Equal(1.02f, opts.RepeatPenalty, precision: 3);
    }

    [Fact]
    public void MaxTokensShouldBeSettable()
    {
        // Act
        var opts = new GenerationOptions { MaxTokens = 4096 };

        // Assert
        Assert.Equal(4096, opts.MaxTokens);
    }

    [Fact]
    public void TemperatureShouldBeSettable()
    {
        // Act
        var opts = new GenerationOptions { Temperature = 1.5f };

        // Assert
        Assert.Equal(1.5f, opts.Temperature, precision: 3);
    }

    [Fact]
    public void ShouldSupportEquality()
    {
        // Arrange
        var a = new GenerationOptions { MaxTokens = 1000, Temperature = 0.5f };
        var b = new GenerationOptions { MaxTokens = 1000, Temperature = 0.5f };

        // Assert
        Assert.Equal(a, b);
    }

    [Fact]
    public void ShouldDifferWhenMaxTokensDiffer()
    {
        // Arrange
        var a = new GenerationOptions { MaxTokens = 1000 };
        var b = new GenerationOptions { MaxTokens = 2000 };

        // Assert
        Assert.NotEqual(a, b);
    }
}
