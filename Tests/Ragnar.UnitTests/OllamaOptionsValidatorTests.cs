namespace Ragnar.Tests;

public class OllamaOptionsValidatorTests
{
    private readonly OllamaOptionsValidator _validator = new();

    [Fact]
    public void ValidOptionsShouldProduceNoErrors()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "localhost",
            Port = 11434,
            LlmModel = "qwen3.8",
            Timeout = TimeSpan.FromMinutes(30)
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void EmptyHostShouldProduceError()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "",
            Port = 11434,
            LlmModel = "qwen3.8",
            Timeout = TimeSpan.FromMinutes(30)
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OllamaOptions.Host));
    }

    [Fact]
    public void NullHostShouldProduceError()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = null!,
            Port = 11434,
            LlmModel = "qwen3.8",
            Timeout = TimeSpan.FromMinutes(30)
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OllamaOptions.Host));
    }

    [Fact]
    public void PortBelowRangeShouldProduceError()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "localhost",
            Port = 0,
            LlmModel = "qwen3.8",
            Timeout = TimeSpan.FromMinutes(30)
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OllamaOptions.Port));
    }

    [Fact]
    public void PortAboveRangeShouldProduceError()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "localhost",
            Port = 65536,
            LlmModel = "qwen3.8",
            Timeout = TimeSpan.FromMinutes(30)
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OllamaOptions.Port));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(80)]
    [InlineData(443)]
    [InlineData(65535)]
    public void ValidPortShouldNotProducePortError(int port)
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "localhost",
            Port = port,
            LlmModel = "qwen3.8",
            Timeout = TimeSpan.FromMinutes(30)
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(OllamaOptions.Port));
    }

    [Fact]
    public void ZeroTimeoutShouldProduceError()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "localhost",
            Port = 11434,
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
            Host = "localhost",
            Port = 11434,
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
            Host = "localhost",
            Port = 11434,
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
            Host = "localhost",
            Port = 11434,
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
            Host = "",
            Port = 0,
            LlmModel = "",
            Timeout = TimeSpan.Zero
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(4, result.Errors.Count);
    }
}
