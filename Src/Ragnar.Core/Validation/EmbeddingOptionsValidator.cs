namespace Ragnar.Core.Validation;

public class EmbeddingOptionsValidator : AbstractValidator<EmbeddingOptions>
{
    public EmbeddingOptionsValidator()
    {
        RuleFor(x => x.BatchSize).NotEmpty().InclusiveBetween(1, 512);
        RuleFor(x => x.EmbeddingModel).NotEmpty().MinimumLength(2).MaximumLength(256);
        RuleFor(x => x.Host).NotEmpty();
        RuleFor(x => x.Timeout).NotEmpty()
            .ExclusiveBetween(TimeSpan.FromMilliseconds(1), TimeSpan.FromHours(1));
    }
}
