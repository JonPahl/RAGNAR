namespace Ragnar.Tests.Validators;

public class OllamaOptionsValidatorTests
{
    private readonly OllamaOptionsValidator _validator = new();


    //[Theory]
    //[InlineData("localhost", 11434, TimeSpan.FromMinutes(20), "qwen3.6")]
    //public void Validate_Should_Pass_When_All_Fields_Are_Valid(string host, int port, TimeSpan timeout, string model)
    //{
    //    var options = new OllamaOptions { Host = host, Port = port, Timeout = timeout, LlmModel = model };
    //    _validator.ShouldNotHaveAnyValidationErrors(options);
    //}

    //[Theory]
    //[InlineData("", 11434, "00:20:00", "qwen3.6")] // Empty Host
    //[InlineData("host", 0, "00:20:00", "qwen3.6")] // Port < 1
    //[InlineData("host", 70000, "00:20:00", "qwen3.6")] // Port > 65535
    //[InlineData("host", 11434, "00:00:00", "qwen3.6")] // Timeout <= 0
    //[InlineData("host", 11434, "00:20:00", "")] // Empty Model
    //public void Validate_Should_Fail_When_Invalid_Field(string Host, int Port, string Timeout, string Model)
    //{
    //    var options = new OllamaOptions
    //    {
    //        Host = Host,
    //        Port = Port,
    //        Timeout = TimeSpan.Parse(Timeout),
    //        LlmModel = Model
    //    };

    //    if (string.IsNullOrEmpty(Host)) _validator.ShouldHaveValidationErrorFor(x => x.Host)(options);
    //    else if (Port is < 1 or > 65535) _validator.ShouldHaveValidationErrorFor(x => x.Port)(options);
    //    else if (Timeout <= TimeSpan.Zero) _validator.ShouldHaveValidationErrorFor(x => x.Timeout)(options);
    //    else _validator.ShouldHaveValidationErrorFor(x => x.LlmModel)(options);
    //}


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
    public void Validate_NullOrEmptyHost_ReturnsInvalid(string? Host)
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
    public void Validate_NullOrEmptyLlmModel_ReturnsInvalid(string? Model)
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
