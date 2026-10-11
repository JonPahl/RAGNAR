namespace Ragnar.Core.Validation;

/// <summary>Initialises validation rules for file-loading options.</summary>
/// <example><![CDATA[var v = new FileLoadOptionsValidator();]]></example>
public class FileLoadOptionsValidator
    : AbstractValidator<FileLoadOptions>
{
    /// <summary>Initializes validation rules for file-loading options.</summary>
    /// <example><![CDATA[var v = new FileLoadOptionsValidator();]]></example>
    public FileLoadOptionsValidator()
    {
        RuleFor(x => x.AllowedExtensions)
            .NotEmpty().WithMessage("At least one allowed extension is required.")
            .Must(exts => exts.All(e => !string.IsNullOrWhiteSpace(e) && e.Length > 1 && e[0] == '.'))
            .WithMessage("Every entry in AllowedExtensions must start with '.' and be longer than 1 char (e.g. \".cs\").");

        RuleFor(x => x.ExcludedFiles)
            .Must(files => files.All(f => !string.IsNullOrWhiteSpace(f)))
                .WithMessage("ExcludedFiles entries must be non-empty file names.");

        RuleFor(x => x.ExcludedDirectories)
            .Must(dirs => dirs.All(d => !string.IsNullOrWhiteSpace(d)
              && !d.Contains('\\') && !d.Contains('/')))
                .WithMessage("ExcludedDirectories must contain folder names only (no path separators).");
    }
}
