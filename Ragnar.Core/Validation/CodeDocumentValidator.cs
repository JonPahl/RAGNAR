namespace Ragnar.Core.Validation;

/// <summary>
/// Validator for CodeDocument instances.
/// </summary>
public class CodeDocumentValidator
    : AbstractValidator<CodeDocument>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CodeDocumentValidator"/> class.
    /// </summary>
    public CodeDocumentValidator()
    {
        RuleFor(Cd => Cd.FileName)
            .NotEmpty()
            .WithMessage("FileName is required.");

        RuleFor(Cd => Cd.Code)
            .NotEmpty()
            .WithMessage("Code content is required.");

        RuleFor(Cd => Cd.CommentLength)
            .GreaterThanOrEqualTo(0)
            .WithMessage("CommentLength must be non-negative.");
    }
}