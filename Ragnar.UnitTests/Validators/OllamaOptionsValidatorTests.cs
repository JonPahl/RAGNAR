namespace Ragnar.Tests.Validators;

public class OllamaOptionsValidatorTests
{
    private readonly OllamaOptionsValidator _validator = new();

    [Fact]
    public void Validate_ValidConfiguration_ReturnsValid()
    {
        var config = new OllamaOptions
        {
            Host = "localhost",
            Port = 11434,
            Timeout = TimeSpan.FromMinutes(20),
            LlmModel = "qwen3.6"
        };

        var result = _validator.Validate(config);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_NullOrEmptyHost_ReturnsInvalid(string Host)
    {
        var config = new OllamaOptions { Host = Host, Port = 11434, Timeout = TimeSpan.FromMinutes(20), LlmModel = "qwen3.6" };
        var result = _validator.Validate(config);
        result.IsValid.Should().BeFalse();
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OllamaOptions.Host));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(65536)]
    public void Validate_InvalidPort_ReturnsInvalid(int Port)
    {
        var config = new OllamaOptions { Host = "localhost", Port = Port, Timeout = TimeSpan.FromMinutes(20), LlmModel = "qwen3.6" };
        var result = _validator.Validate(config);
        result.IsValid.Should().BeFalse();
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OllamaOptions.Port));
    }

    [Fact]
    public void Validate_ZeroTimeout_ReturnsInvalid()
    {
        var config = new OllamaOptions { Host = "localhost", Port = 11434, Timeout = TimeSpan.Zero, LlmModel = "qwen3.6" };
        var result = _validator.Validate(config);
        result.IsValid.Should().BeFalse();
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OllamaOptions.Timeout));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_NullOrEmptyLlmModel_ReturnsInvalid(string Model)
    {
        var config = new OllamaOptions { Host = "localhost", Port = 11434, Timeout = TimeSpan.FromMinutes(20), LlmModel = Model };
        var result = _validator.Validate(config);
        result.IsValid.Should().BeFalse();
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(OllamaOptions.LlmModel));
    }

    [Fact]
    public void Validate_UsingTestValidate_ReturnsExpectedErrors()
    {
        var config = new OllamaOptions { Host = "", Port = 0, Timeout = TimeSpan.Zero, LlmModel = "" };
        var result = _validator.TestValidate(config);
        result.ShouldHaveValidationErrorFor(x => x.Host);
        result.ShouldHaveValidationErrorFor(x => x.Port);
        result.ShouldHaveValidationErrorFor(x => x.Timeout);
        result.ShouldHaveValidationErrorFor(x => x.LlmModel);
    }
}
