namespace Ragnar.Validator;

/// <summary>Validates ApplicationOptions for structural correctness.</summary>
/// <example><![CDATA[var validator = new ApplicationOptionsValidation();]]>
/// </example>
public sealed class ApplicationOptionsValidation
    : AbstractValidator<ApplicationOptions>
{

    /// <summary>Initializes FluentValidation rules for application options.</summary>
    /// <example><![CDATA[var v = new ApplicationOptionsValidation();]]></example>
    public ApplicationOptionsValidation()
    {
        RuleFor(x => x.VectorStoreName)
            .NotEmpty().WithMessage("Qdrant Vector Store Name is required.")
            .MaximumLength(128).WithMessage("VectorStoreName must not exceed 128 characters.");

        RuleFor(x => x.SourceDirectory)
            .NotEmpty().WithMessage("SourceDirectory is required.")
            .MaximumLength(1024).WithMessage("SourceDirectory path must not exceed 1024 characters.");

        RuleFor(x => x.OutputFolder)
            .NotEmpty().WithMessage("OutputFolder is required.");

    }
}
