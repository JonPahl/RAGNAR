namespace Ragnar.Validator;

/// <summary>Initializes validation rules for application-level options.</summary>
/// <example><![CDATA[[var v = new ApplicationOptionsValidator();]]></example>
public class ApplicationOptionsValidator
    : AbstractValidator<ApplicationOptions>
{
    private static readonly Regex _validFolderName = new(@"^[\w.-]+$", RegexOptions.Compiled);

    public ApplicationOptionsValidator()
    {
        RuleFor(x => x.OutputFolder)
            .NotEmpty().WithMessage("OutputFolder is required.")
            .Matches(_validFolderName).WithMessage("OutputFolder must not contain path separators or invalid characters.");

        RuleFor(x => x.SourceDirectory)
            .NotEmpty().WithMessage("SourceDirectory is required.");

        // Note: Directory.Exists is environment-specific; validate at runtime if needed.

        RuleFor(x => x.VectorStoreName)
            .NotEmpty().WithMessage("VectorStoreName is required.")
            .Matches(@"^[[a-zA-Z0-9_]]+$").WithMessage("VectorStoreName must be alphanumeric with underscores.");
    }
}
