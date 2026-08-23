namespace Ragnar.Validator;

/// <summary>
/// Validates configuration settings for Ollama API connections and timeouts.
/// </summary>
public class OllamaOptionsValidator
    : AbstractValidator<OllamaOptions>
{
    ///<summary>Initializes a new instance of the validator with default rules. </summary>
    public OllamaOptionsValidator()
    {
        RuleFor(x => x.Host).NotEmpty().WithMessage("Host is required.");
        RuleFor(x => x.Port).InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535.");
        RuleFor(x => x.Timeout).GreaterThan(TimeSpan.Zero).WithMessage("Timeout must be greater than zero.");
        RuleFor(x => x.LlmModel).NotEmpty().WithMessage("LLM model is required.");
    }
}
