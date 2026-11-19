namespace Ragnar.Validator;

/// <summary>Validates configuration options for embedding services.</summary>
/// <remarks>This validator enforces constraints on host connectivity, model specifications, timeout limits, and batch processing parameters to ensure valid service initialization.</remarks>
public partial class EmbeddingOptionsValidator : AbstractValidator<EmbeddingOptions>
{
    /// <summary>Initializes validation rules for embedding options.</summary>
    public EmbeddingOptionsValidator()
    {
        RuleFor(x => x.Host)
            .NotEmpty().WithMessage("Host is required.");

        RuleFor(x => x.EmbeddingModel)
            .NotEmpty().WithMessage("Embedding model is required.")
            .MaximumLength(256).WithMessage("Embedding model name must be 256 characters or fewer.");

        RuleFor(x => x.Timeout)
            .GreaterThan(TimeSpan.Zero).WithMessage("Timeout must be greater than zero.")
            .LessThan(TimeSpan.FromHours(1)).WithMessage("Timeout must be less than one hour.");

        RuleFor(x => x.Dimension)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.BatchSize)
            .NotEmpty().NotNull()
            .InclusiveBetween(1, 512).WithMessage("BatchSize must be between 1 and 512.");
    }

    /// <summary>Defines the regular expression pattern used to validate string identifiers (e.g., model names, hostnames).</summary>
    [GeneratedRegex("^[[a-zA-Z0-9._-]]+$")]
    private static partial Regex StringRegex();
}
