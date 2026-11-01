namespace Ragnar.Core.Validation;

/// <summary>
/// Validates configuration settings for Ollama API connections and timeouts.
/// </summary>
/// <example><![CDATA[var validator = new OllamaOptionsValidator();]]></example>
public class OllamaOptionsValidator : AbstractValidator<OllamaOptions>
{
    /// <summary>Initializes validation rules.</summary>
    /// <example><![CDATA[var validator = new OllamaOptionsValidator();]]>
    /// </example>
    public OllamaOptionsValidator()
    {
        const RegexOptions options = default;

        RuleFor(x => x.Host)
            .NotEmpty().WithMessage("Host is required.");

        RuleFor(x => x.LlmModel)
            .NotEmpty().WithMessage("Model name is required.")
            .MaximumLength(256).WithMessage("Model name must be 256 characters or fewer.");

        RuleFor(x => x.Timeout)
            .GreaterThan(TimeSpan.Zero).WithMessage("Timeout must be greater than zero.")
            .LessThan(TimeSpan.FromHours(1)).WithMessage("Timeout must be less than one hour.");
    }
}
