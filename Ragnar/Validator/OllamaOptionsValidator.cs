namespace Ragnar.Validator;

// Example Validator matching your OllamaOptions/EmbeddingOptions structure
public class OllamaOptionsValidator : AbstractValidator<OllamaOptions>
{
    public OllamaOptionsValidator()
    {
        RuleFor(x => x.Host).NotEmpty().WithMessage("Host is required.");
        RuleFor(x => x.Port).InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535.");
        RuleFor(x => x.Timeout).GreaterThan(TimeSpan.Zero).WithMessage("Timeout must be greater than zero.");
    }
}
