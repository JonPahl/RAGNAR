namespace Ragnar.Tests;

public class GenerationOptionsTests
{
    [Fact]
    public void DefaultValuesShouldMatchExpectedConstants()
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
    public void CustomValuesShouldBeRespected()
    {
        // Act
        var opts = new GenerationOptions
        {
            MaxTokens = 1024,
            ContextWindow = 131_072,
            ThreadCount = 4,
            Temperature = 0.7f,
            RepeatPenalty = 1.1f
        };

        // Assert
        Assert.Equal(1024, opts.MaxTokens);
        Assert.Equal(131_072, opts.ContextWindow);
        Assert.Equal(4, opts.ThreadCount);
        Assert.Equal(0.7f, opts.Temperature);
        Assert.Equal(1.1f, opts.RepeatPenalty);
    }

    //[Fact]
    //public void MaxTokensBelowMinimumShouldFailValidation()
    //{
    //    // Arrange
    //    var opts = new GenerationOptions { MaxTokens = 0 };
    //    var ctx = new ValidationContext(opts);
    //    var results = new List<ValidationResult>();

    //    // Act
    //    var valid = Validator.TryValidateObject(opts, ctx, results);

    //    // Assert
    //    Assert.False(valid);
    //    Assert.Contains(results, r => r.MemberNames!.Any(n => n.Contains("MaxTokens")));
    //}


    [Fact]
    public void RecordEqualityShouldWorkForIdenticalOptions()
    {
        // Arrange
        var a = new GenerationOptions { MaxTokens = 100, Temperature = 0.5f };
        var b = new GenerationOptions { MaxTokens = 100, Temperature = 0.5f };

        // Act & Assert
        Assert.Equal(a, b);
    }

    [Fact]
    public void RecordEqualityShouldFailWhenValuesDiffer()
    {
        // Arrange
        var a = new GenerationOptions { MaxTokens = 100 };
        var b = new GenerationOptions { MaxTokens = 200 };

        // Act & Assert
        Assert.NotEqual(a, b);
    }
}
