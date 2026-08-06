namespace Ragnar.Core.Validation;

public class QuestionValidator
    : AbstractValidator<Question>
{
    /// <summary>Validates question data for required fields.</summary>
    /// <example><![CDATA[new QuestionValidator()]]></example>
    public QuestionValidator()
    {
        RuleFor(Q => Q.Text).NotEmpty().WithMessage("Text is required.");

        RuleFor(Q => Q.Filename)
            .NotEmpty()
            .WithMessage("Filename is required.");

        RuleFor(Q => Q.Category)
            .IsInEnum()
            .WithMessage("Invalid question category.");
    }
}
