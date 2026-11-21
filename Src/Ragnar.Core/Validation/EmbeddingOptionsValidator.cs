namespace Ragnar.Core.Validation;

/// <summary>Validates that embedding batch size is configured and non-empty.</summary>
/// <example><![CDATA[v = new EmbeddingOptionsValidator();]></example>
public class EmbeddingOptionsValidator
    : AbstractValidator<EmbeddingOptions>
{
    /// <summary>Initializes validators for EmbeddingOptions configuration values.</summary>
    /// <example><![CDATA[var v = new EmbeddingOptionsValidator();]]></example>
    public EmbeddingOptionsValidator()
    {
        RuleFor(x => x.BatchSize).NotEmpty();
    }
}
