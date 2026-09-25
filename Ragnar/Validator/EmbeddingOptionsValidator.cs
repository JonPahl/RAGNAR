namespace Ragnar.Validator;

/// <summary>Validates embedding service configuration.</summary>
/// <example><![CDATA[var v = new EmbeddingOptionsValidator();]]></example>
public class EmbeddingOptionsValidator
    : AbstractValidator<EmbeddingOptions>
{
    public EmbeddingOptionsValidator()
    {
        RuleFor(x => x.Host)
            .NotEmpty().WithMessage("Host is required.")
            .Matches(new Regex(@"^[[a-zA-Z0-9._-]]+$")).WithMessage("Host must be a valid hostname or IP.");

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535.");

        RuleFor(x => x.EmbeddingModel)
            .NotEmpty().WithMessage("Embedding model is required.")
            .MaximumLength(256).WithMessage("Embedding model name must be 256 characters or fewer.");

        RuleFor(x => x.Timeout)
            .GreaterThan(TimeSpan.Zero).WithMessage("Timeout must be greater than zero.")
            .LessThan(TimeSpan.FromHours(1)).WithMessage("Timeout must be less than one hour.");

        RuleFor(x => x.Dimension)
            .NotNull()
            .NotEmpty()
        //    .InclusiveBetween(1, 65536).WithMessage("Dimension must be between 1 and 65536.")
        ;

        RuleFor(x => x.BatchSize)
            .NotEmpty().NotNull()
            .InclusiveBetween(1, 512).WithMessage("BatchSize must be between 1 and 512.");
    }
}
