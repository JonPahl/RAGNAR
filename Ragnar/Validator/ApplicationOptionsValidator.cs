namespace Ragnar.Validator;

/// <summary>Validates application-level options (paths, categories, toggles).</summary>
/// <example><![CDATA[var v = new ApplicationOptionsValidator();]]></example>
public class ApplicationOptionsValidator
    : AbstractValidator<ApplicationOptions>
{
    private static readonly Regex _validFolderName = new(@"^[[\w.-]]+$", RegexOptions.Compiled);

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

        RuleFor(x => x.CategoriesToProcess)
            .NotEmpty().WithMessage("CategoriesToProcess must contain at least one category.");

        RuleForEach(x => x.CategoriesToProcess)
            .NotEmpty().WithMessage("Each category entry must be non-empty.");
    }
}
