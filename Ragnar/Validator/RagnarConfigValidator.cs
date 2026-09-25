namespace Ragnar.Validator;

/// <summary>Validates the root configuration record and cascades to child options.</summary>
/// <example><![CDATA[var v = new RagnarConfigValidator();]]></example>
public class RagnarConfigValidator
    : AbstractValidator<RagnarConfig>
{
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

        // Cascade: only validate children if they are non-null
        RuleFor(x => x.ApplicationOptions)
            .SetValidator(appValidator)
            //.OverrideChildRules()
            ;

        RuleFor(x => x.EmbeddingOptions)
            .SetValidator(embedValidator);

        RuleFor(x => x.OllamaOptions)
            .SetValidator(ollamaValidator);

        RuleFor(x => x.FileLoadOptions)
            .SetValidator(fileValidator);
    }
}

