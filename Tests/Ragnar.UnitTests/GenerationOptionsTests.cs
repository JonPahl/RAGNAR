namespace Ragnar.Tests;

public class GenerationOptionsTests
{
    [Fact]
    public void DefaultMaxTokensShouldBe32768()
    {
        // Act
        var opts = new GenerationOptions();

        // Assert
        Assert.Equal(32_768, opts.MaxTokens);
    }

    [Fact]
    public void DefaultContextWindowShouldBe65536()
    {
        // Act
        var opts = new GenerationOptions();

        // Assert
        Assert.Equal(65_536, opts.ContextWindow);
    }

    [Fact]
    public void DefaultThreadCountShouldBe8()
    {
        // Act
        var opts = new GenerationOptions();

        // Assert
        Assert.Equal(8, opts.ThreadCount);
    }

    [Fact]
    public void DefaultTemperatureShouldBe02()
    {
        // Act
        var opts = new GenerationOptions();

        // Assert
        Assert.Equal(0.2f, opts.Temperature);
    }

    [Fact]
    public void DefaultRepeatPenaltyShouldBe102()
    {
        // Act
        var opts = new GenerationOptions();

        // Assert
        Assert.Equal(1.02f, opts.RepeatPenalty);
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
    public void ShouldSupportImmutability()
    {
        // Arrange
        var original = new GenerationOptions { MaxTokens = 1000, Temperature = 0.5f };

        // Act
        var modified = original with { MaxTokens = 2000 };

        // Assert
        Assert.Equal(1000, original.MaxTokens);
        Assert.Equal(2000, modified.MaxTokens);
    }

    [Fact]
    public void EqualityShouldWorkWhenAllPropertiesMatch()
    {
        // Arrange
        var a = new GenerationOptions { MaxTokens = 5000, ThreadCount = 4 };
        var b = new GenerationOptions { MaxTokens = 5000, ThreadCount = 4 };

        // Act & Assert
        Assert.Equal(a, b);
    }

    [Fact]
    public void InequalityShouldWorkWhenPropertyDiffers()
    {
        // Arrange
        var a = new GenerationOptions { MaxTokens = 5000 };
        var b = new GenerationOptions { MaxTokens = 6000 };

        // Act & Assert
        Assert.NotEqual(a, b);
    }
}
