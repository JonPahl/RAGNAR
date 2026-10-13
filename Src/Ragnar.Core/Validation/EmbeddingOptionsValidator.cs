namespace Ragnar.Core.Validation;

public class EmbeddingOptionsValidator : AbstractValidator<EmbeddingOptions>
{
    public EmbeddingOptionsValidator()
    {

        // ApplyRulesFromAttributes();

        //RuleFor(x => x.BatchSize)
        //.Must((opts, size) => size >= 1 && size <= opts.Dimension)
        //.WithMessage("BatchSize must not exceed the embedding Dimension.");
    }
}
