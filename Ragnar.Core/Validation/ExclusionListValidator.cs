namespace Ragnar.Core.Validation;

/// <summary> Validator for IReadOnlyCollection string instances. </summary>
public class ExclusionListValidator
    : AbstractValidator<IReadOnlyCollection<string>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExclusionListValidator"/> class.
    /// </summary>
    public ExclusionListValidator()
    {
        RuleFor(List => List)
            .NotNull()
            .WithMessage("Exclusions collection cannot be null.");

        RuleForEach(List => List)
            .NotEmpty()
            .WithMessage("Exclusion patterns must not be empty.");
    }
}