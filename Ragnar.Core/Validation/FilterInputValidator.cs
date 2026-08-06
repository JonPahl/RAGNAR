namespace Ragnar.Core.Validation;

public class FilterInputValidator
    : AbstractValidator<(QuestionCategory Category, int Size)>
{
    public FilterInputValidator()
    {
        RuleFor(X => X.Category)
            .IsInEnum()
            .WithMessage("Invalid question category.");

        RuleFor(X => X.Size)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Size must be non-negative.");
    }
}

