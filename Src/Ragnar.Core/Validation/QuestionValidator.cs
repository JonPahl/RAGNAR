namespace Ragnar.Core.Validation;

/// <summary>
/// FluentValidation rules for the Question model (text, filename, category, filter).
/// </summary>
/// <example><![CDATA[[var v = new QuestionValidator(); var res = v.Validate(question);]]></example>
public sealed class QuestionValidator
    : AbstractValidator<Models.Question>
{
    /// <summary>Initializes validation rules for the Question model.</summary>
    /// <example><![CDATA[var v = new QuestionValidator();]]></example>
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

