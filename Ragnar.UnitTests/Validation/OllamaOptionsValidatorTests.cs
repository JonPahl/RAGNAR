namespace Ragnar.Tests.Validation;

public class OllamaOptionsValidatorTests
{
    private readonly OllamaOptionsValidator _validator;

    public OllamaOptionsValidatorTests()
    {
        _validator = new OllamaOptionsValidator();
    }

    [Fact]
    public void ValidateValidOptionsShouldHaveNoErrors()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "localhost",
            Port = 11434,
            Timeout = TimeSpan.FromMinutes(30),
            LlmModel = "qwen3.8"
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidateEmptyHostShouldFail()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "",
            Port = 11434,
            Timeout = TimeSpan.FromMinutes(30),
            LlmModel = "qwen3.8"
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor("Host");
    }

    [Fact]
    public void ValidatePortTooLowShouldFail()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "localhost",
            Port = 0,
            Timeout = TimeSpan.FromMinutes(30),
            LlmModel = "qwen3.8"
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor("Port");
    }

    [Fact]
    public void ValidatePortTooHighShouldFail()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "localhost",
            Port = 65536,
            Timeout = TimeSpan.FromMinutes(30),
            LlmModel = "qwen3.8"
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor("Port");
    }

    [Fact]
    public void ValidatePortAtBoundariesShouldPass()
    {
        // Act & Assert
        var low = _validator.TestValidate(new OllamaOptions
        {
            Host = "localhost",
            Port = 1,
            Timeout = TimeSpan.FromMinutes(1),
            LlmModel = "m"
        });
        low.ShouldNotHaveValidationErrorFor("Port");

        var high = _validator.TestValidate(new OllamaOptions
        {
            Host = "localhost",
            Port = 65535,
            Timeout = TimeSpan.FromMinutes(1),
            LlmModel = "m"
        });
        high.ShouldNotHaveValidationErrorFor("Port");
    }

    [Fact]
    public void ValidateZeroTimeoutShouldFail()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "localhost",
            Port = 11434,
            Timeout = TimeSpan.Zero,
            LlmModel = "qwen3.8"
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor("Timeout");
    }

    [Fact]
    public void ValidateEmptyLlmModelShouldFail()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "localhost",
            Port = 11434,
            Timeout = TimeSpan.FromMinutes(30),
            LlmModel = ""
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor("LlmModel");
    }

    [Fact]
    public void ValidateAllInvalidShouldHaveMultipleErrors()
    {
        // Arrange
        var options = new OllamaOptions
        {
            Host = "",
            Port = 0,
            Timeout = TimeSpan.Zero,
            LlmModel = ""
        };

        // Act
        var result = _validator.TestValidate(options);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Host);
        result.ShouldHaveValidationErrorFor(x => x.Port);
        result.ShouldHaveValidationErrorFor(x => x.Timeout);
        result.ShouldHaveValidationErrorFor(x => x.LlmModel);
    }
}
