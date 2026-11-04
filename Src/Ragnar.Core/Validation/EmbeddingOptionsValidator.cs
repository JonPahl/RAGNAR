namespace Ragnar.Core.Validation;

public class EmbeddingOptionsValidator : AbstractValidator<EmbeddingOptions>
{
    public EmbeddingOptionsValidator()
    {
        RuleFor(x => x.BatchSize).NotEmpty();
    }
}
