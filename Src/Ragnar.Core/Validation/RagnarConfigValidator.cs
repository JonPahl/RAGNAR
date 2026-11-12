namespace Ragnar.Core.Validation;

/// <summary>Validates all RagnarConfig child option records in a single cascade.</summary>
/// <example><![CDATA[var v = new RagnarConfigValidator(appV, embV, ollV, fileV);]]></example>
public class RagnarConfigValidator : AbstractValidator<RagnarConfig>
{

    /// <summary>Initializes validation rules and composes nested option validators.</summary>
    /// <param name = "embedValidator">Validator for embedding options.</param>
    /// <param name = "ollamaValidator">Validator for Ollama options.</param>
    /// <param name = "fileValidator">Validator for file-loading options.</param>
    /// <example><![CDATA[var v = new RagnarConfigValidator(ev, ov, fv);]]></example>
    public RagnarConfigValidator(
        EmbeddingOptionsValidator embedValidator,
        OllamaOptionsValidator ollamaValidator,
        FileLoadOptionsValidator fileValidator)
    {
        RuleFor(x => x.ApplicationOptions).NotNull().WithMessage("ApplicationOptions is required.");
        RuleFor(x => x.EmbeddingOptions).NotNull().WithMessage("EmbeddingOptions is required.");
        RuleFor(x => x.OllamaOptions).NotNull().WithMessage("OllamaOptions is required.");
        RuleFor(x => x.FileLoadOptions).NotNull().WithMessage("FileLoadOptions is required.");

        RuleFor(x => x.EmbeddingOptions).SetValidator(embedValidator);
        RuleFor(x => x.OllamaOptions).SetValidator(ollamaValidator);
        RuleFor(x => x.FileLoadOptions).SetValidator(fileValidator);
    }
}
