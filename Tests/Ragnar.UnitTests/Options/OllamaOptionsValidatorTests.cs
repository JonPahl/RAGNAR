namespace Ragnar.Tests.Options;

public class OllamaOptionsValidatorTests
{
    private readonly OllamaOptionsValidator _validator = new();

    [Fact]
    public void ValidOptionsShouldProduceNoErrors()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = new Uri("http://localhost:11434/"),
            LlmModel = "qwen3.8",
            Timeout = TimeSpan.FromMinutes(30)
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ZeroTimeoutShouldProduceError()
    {
        // Arrange 
        var options = new OllamaOptions
        {
            Host = new Uri("http://localhost:11434/"),
            LlmModel = "qwen3.8",
            Timeout = TimeSpan.Zero
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OllamaOptions.Timeout));
    }

    [Fact]
    public void NegativeTimeoutShouldProduceError()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = new Uri("http://localhost:11434/"),
            LlmModel = "qwen3.8",
            Timeout = TimeSpan.FromSeconds(-1)
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OllamaOptions.Timeout));
    }

    [Fact]
    public void EmptyLlmModelShouldProduceError()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = new Uri("http://localhost:11434/"),
            LlmModel = "",
            Timeout = TimeSpan.FromMinutes(30)
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OllamaOptions.LlmModel));
    }

    [Fact]
    public void NullLlmModelShouldProduceError()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = new Uri("http://localhost:11434/"),
            LlmModel = null!,
            Timeout = TimeSpan.FromMinutes(30)
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OllamaOptions.LlmModel));
    }

    [Fact]
    public void AllFieldsInvalidShouldProduceMultipleErrors()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = new Uri("http://localhost:11434/"),
            LlmModel = "",
            Timeout = TimeSpan.Zero
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }
}
