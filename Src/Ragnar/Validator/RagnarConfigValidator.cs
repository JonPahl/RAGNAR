namespace Ragnar.Validator;

public class RagnarConfigValidator
    : AbstractValidator<RagnarConfig>
{
    /// <summary>Initializes cascade validation rules for all child option records.
    /// </summary>
    /// <param name="appValidator">Validator for ApplicationOptions.</param>
    /// <param name="embedValidator">Validator for EmbeddingOptions.</param>
    /// <param name="ollamaValidator">Validator for OllamaOptions.</param>
    /// <param name="fileValidator">Validator for FileLoadOptions.</param>
    /// <example>
    /// <![CDATA[var v = new RagnarConfigValidator(appV, embV, ollV, fileV);]]>
    /// </example>
    public RagnarConfigValidator(
        ApplicationOptionsValidator appValidator,
        EmbeddingOptionsValidator embedValidator,
        OllamaOptionsValidator ollamaValidator,
        FileLoadOptionsValidator fileValidator)
    {
        RuleFor(x => x.ApplicationOptions).NotNull().WithMessage("ApplicationOptions is required.");
        RuleFor(x => x.EmbeddingOptions).NotNull().WithMessage("EmbeddingOptions is required.");
        RuleFor(x => x.OllamaOptions).NotNull().WithMessage("OllamaOptions is required.");
        RuleFor(x => x.FileLoadOptions).NotNull().WithMessage("FileLoadOptions is required.");

        RuleFor(x => x.ApplicationOptions)
            .SetValidator(appValidator);

        RuleFor(x => x.EmbeddingOptions)
            .SetValidator(embedValidator);

        RuleFor(x => x.OllamaOptions)
            .SetValidator(ollamaValidator);

        RuleFor(x => x.FileLoadOptions)
            .SetValidator(fileValidator);
    }
}
