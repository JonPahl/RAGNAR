namespace Ragnar.Tests;

public class ConfigurationValidatorTests
{
    private readonly OllamaOptionsValidator _validator = new();

    [Fact]
    public void Validate_ValidOptions_ReturnsNoErrors()
    {
        var options = new OllamaOptions
        {
            Host = "localhost",
            Port = 11434,
            Timeout = TimeSpan.FromMinutes(20),
            LlmModel = "model"
        };

        var result = _validator.Validate(options);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidOptions_ReturnsErrors()
    {
        var options = new OllamaOptions { Host = "", Port = 99999, Timeout = TimeSpan.Zero, LlmModel = "" };

        var result = _validator.Validate(options);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4);

        result.Errors.First().PropertyName.Should().Be("Host");
    }
}
