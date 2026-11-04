namespace Ragnar.Tests.Options;

public class GenerationOptionsTests
{
    [Fact]
    public void DefaultsShouldHaveExpectedValues()
    {
        // Act
        var opts = new GenerationOptions();

        // Assert
        Assert.Equal(32_768, opts.MaxTokens);
        Assert.Equal(65_536, opts.ContextWindow);
        Assert.Equal(8, opts.ThreadCount);
        Assert.Equal(0.2f, opts.Temperature);
        Assert.Equal(1.02f, opts.RepeatPenalty);
    }

    [Fact]
    public void MaxTokensShouldAcceptValidValues()
    {
        // Act
        var opts = new GenerationOptions { MaxTokens = 4096 };

        // Assert
        Assert.Equal(4096, opts.MaxTokens);
    }

    [Fact]
    public void MaxTokensShouldRejectZero()
    {
        // Act
        var opts = new GenerationOptions { MaxTokens = 0 };

        // Assert
        var context = new ValidationContext(opts);
        var results = new List<ValidationResult>();
        Assert.False(System.ComponentModel.DataAnnotations.Validator.TryValidateObject(opts, context, results, validateAllProperties: true));
    }

    [Fact]
    public void MaxTokensShouldRejectNegativeValues()
    {
        // Act
        var opts = new GenerationOptions { MaxTokens = -1 };

        // Assert
        var context = new ValidationContext(opts);
        var results = new List<ValidationResult>();
        Assert.False(System.ComponentModel.DataAnnotations.Validator.TryValidateObject(opts, context, results, validateAllProperties: true));
    }

    [Fact]
    public void ContextWindowShouldAcceptValidValues()
    {
        // Act
        var opts = new GenerationOptions { ContextWindow = 131072 };

        // Assert
        Assert.Equal(131072, opts.ContextWindow);
    }

    [Fact]
    public void ThreadCountShouldAcceptValidValues()
    {
        // Act
        var opts = new GenerationOptions { ThreadCount = 16 };

        // Assert
        Assert.Equal(16, opts.ThreadCount);
    }

    [Fact]
    public void TemperatureShouldAcceptZero()
    {
        // Act
        var opts = new GenerationOptions { Temperature = 0.0f };

        // Assert
        Assert.Equal(0.0f, opts.Temperature);
    }

    [Fact]
    public void TemperatureShouldRejectAboveTwo()
    {
        // Act
        var opts = new GenerationOptions { Temperature = 2.5f };

        // Assert

        var context = new ValidationContext(opts, serviceProvider: null, items: null);

        var results = new List<ValidationResult>();
        var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(opts, context, results, validateAllProperties: true);

        Assert.False(isValid);
    }

    [Fact]
    public void RepeatPenaltyShouldRejectBelowOne()
    {
        // Act
        var opts = new GenerationOptions { RepeatPenalty = 0.5f };

        // Assert
        var context = new ValidationContext(opts);
        var results = new List<ValidationResult>();
        Assert.False(System.ComponentModel.DataAnnotations.Validator.TryValidateObject(opts, context, results, validateAllProperties: true));
    }

    [Fact]
    public void GenerationOptionsShouldSupportEquality()
    {
        // Arrange
        var a = new GenerationOptions { MaxTokens = 1024, Temperature = 0.5f };
        var b = new GenerationOptions { MaxTokens = 1024, Temperature = 0.5f };

        // Assert
        Assert.Equal(a, b);
    }

    [Fact]
    public void GenerationOptionsShouldDifferWhenMaxTokensDiffers()
    {
        // Arrange
        var a = new GenerationOptions { MaxTokens = 1024 };
        var b = new GenerationOptions { MaxTokens = 2048 };

        // Assert
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void DefaultsShouldBeCorrect()
    {
        var opts = new GenerationOptions();

        Assert.Equal(32_768, opts.MaxTokens);
        Assert.Equal(65_536, opts.ContextWindow);
        Assert.Equal(8, opts.ThreadCount);
        Assert.Equal(0.2f, opts.Temperature);
        Assert.Equal(1.02f, opts.RepeatPenalty);
    }

    [Fact]
    public void MaxTokensShouldAcceptValidValue()
    {
        var opts = new GenerationOptions { MaxTokens = 4096 };
        Assert.Equal(4096, opts.MaxTokens);
    }

    [Fact]
    public void ContextWindowShouldAcceptValidValue()
    {
        var opts = new GenerationOptions { ContextWindow = 128000 };
        Assert.Equal(128000, opts.ContextWindow);
    }

    [Fact]
    public void ThreadCountShouldAcceptValidValue()
    {
        var opts = new GenerationOptions { ThreadCount = 16 };
        Assert.Equal(16, opts.ThreadCount);
    }

    [Fact]
    public void TemperatureShouldAcceptValidValue()
    {
        var opts = new GenerationOptions { Temperature = 1.5f };
        Assert.Equal(1.5f, opts.Temperature);
    }

    [Fact]
    public void ShouldSupportEquality()
    {
        var a = new GenerationOptions { MaxTokens = 100 };
        var b = new GenerationOptions { MaxTokens = 100 };

        Assert.Equal(a, b);
    }

    [Fact]
    public void ShouldDifferWhenMaxTokensDiffers()
    {
        var a = new GenerationOptions { MaxTokens = 100 };
        var b = new GenerationOptions { MaxTokens = 200 };

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ShouldSupportWithExpression()
    {
        var original = new GenerationOptions { MaxTokens = 4096 };
        var modified = original with { MaxTokens = 8192 };

        Assert.Equal(4096, original.MaxTokens);
        Assert.Equal(8192, modified.MaxTokens);
    }
}
