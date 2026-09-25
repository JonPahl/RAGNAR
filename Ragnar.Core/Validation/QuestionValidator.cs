namespace Ragnar.Core.Validation;

public sealed class QuestionValidator
    : AbstractValidator<Model.Question>
{
    public QuestionValidator()
    {
        RuleFor(q => q.Text)
            .NotEmpty().WithMessage("Text is required.")
            .MaximumLength(4000).WithMessage("Text must be 4 000 characters or fewer.");

        RuleFor(q => q.Filename)
            .NotEmpty().WithMessage("Filename is required.");

        RuleFor(q => q.Category)
            .IsInEnum().WithMessage("Category must be a valid enum value.");

        RuleFor(q => q.IsEnabled)
            .NotNull().WithMessage("IsEnabled must not be null.");

        // Conditional: if a filter exists, it must be well-formed
        RuleFor(q => q.Filter)
            .ChildRules(f =>
            {
                f.RuleFor(x => x.Must)
                    .NotEmpty().WithMessage("Filter must contain at least one Must condition.");
            })
            .When(q => q.Filter is not null);
    }
}

