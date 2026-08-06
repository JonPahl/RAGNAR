namespace Ragnar.Validator;

public class QuestionValidator
    : AbstractValidator<Question>
{
    public QuestionValidator()
    {
        RuleFor(q => q.Text).NotEmpty().WithMessage("Text cannot be null/whitespace.");
        RuleFor(q => q.Filename).NotEmpty().WithMessage("Filename cannot be null/whitespace.");
    }
}
