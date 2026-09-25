namespace Ragnar.Validator;

/// <summary>Initialises validation rules for file-loading options.</summary>
/// <example><![CDATA[var v = new FileLoadOptionsValidator();]]></example>
public class FileLoadOptionsValidator : AbstractValidator<FileLoadOptions>
{
    /// <summary>Initializes validation rules for file-loading options.</summary>
    /// <example><![CDATA[var v = new FileLoadOptionsValidator();]]></example>
    public FileLoadOptionsValidator()
    {
        RuleFor(x => x.AllowedExtensions)
            .NotEmpty().WithMessage("At least one allowed extension is required.");

        //RuleForEach(x => x.AllowedExtensions)
        //    .NotEmpty().WithMessage("Extension entries cannot be empty.")
        //    .Must(ext => ext.StartsWith('.') && ext.Length > 1)
        //    .WithMessage("Each extension must start with '.' (e.g. '.cs').");

        RuleForEach(x => x.AllowedExtensions)
    .Must(ext => !string.IsNullOrWhiteSpace(ext)
               && ext.Length > 1
               && ext[0] == '.')
    .WithMessage("Each allowed extension must be non-empty and start with '.' (e.g. \".cs\").");

        RuleForEach(x => x.ExcludedFiles)
            .NotEmpty().WithMessage("Excluded file entries cannot be empty.");

        RuleForEach(x => x.ExcludedDirectories)
            .NotEmpty().WithMessage("Excluded directory entries cannot be empty.")
            .Must(dir => !dir.Contains('\\') && !dir.Contains('/'))
            .WithMessage("Excluded directories must be folder names only, not full paths.");
    }
}
