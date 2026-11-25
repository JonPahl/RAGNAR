namespace Ragnar.Domain.Validation;

public class EmbeddingOptionsValidator : AbstractValidator<EmbeddingOptions>
{
    public EmbeddingOptionsValidator()
    {
        RuleFor(x => x.BatchSize).NotEmpty();
    }
}
