namespace Ragnar.Factory;

/// <summary>Initializes a new instance of the default question factory.</summary>
public class QuestionBuilder : IQuestionBuilder
{
    /// <summary>Stores the question text to be assigned during Build().</summary>
    private string? _text;

    /// <summary>Stores the source-file key identifier for the question.</summary>
    private string? _key;

    /// <summary>The classification category; defaults to Other.</summary>
    private QuestionCategory _category = QuestionCategory.Other;

    /// <summary>Whether the question is active; defaults to true.</summary>
    private bool _isEnabled = true;

    /// <summary>Sets the question text and returns the builder for chaining.</summary>
    /// <param name="text">The question or prompt text to assign.</param>
    /// <returns>This builder instance for further configuration.</returns>
    /// <example><![CDATA[builder.WithText("Explain this code");]]></example>
    public QuestionBuilder WithText(string text)
    {
        _text = text;
        return this;
    }

    /// <summary>Assigns a source-file key identifier to the question.</summary>
    /// <param name="key">The filename or unique identifier for the question.</param>
    /// <returns>This builder instance for further configuration.</returns>
    /// <example><![CDATA[builder.WithFileName("Program.cs");]]></example>
    public QuestionBuilder WithFileName(string key)
    {
        _key = key;
        return this;
    }

    /// <summary>Sets the question category; pass null to retain the current value.</summary>
    /// <param name="category">The target category, or null to leave unchanged.</param>
    /// <returns>This builder instance for further configuration.</returns>
    /// <example><![CDATA[builder.SetCategory(QuestionCategory.Performance);]]></example>
    public QuestionBuilder SetCategory(QuestionCategory? category)
    {
        if (category is not null)
            _category = category.Value;
        return this;
    }

    /// <summary>Sets the question as active.</summary>
    /// <returns>This builder for further configuration.</returns>
    /// <example><![CDATA[builder.AsActive();]]></example>
    public QuestionBuilder AsActive()
    {
        _isEnabled = true;
        return this;
    }

    /// <summary>Sets the question as inactive.</summary>
    /// <returns>This builder for further configuration.</returns>
    /// <example><![CDATA[builder.AsInactive();]]>
    /// </example>
    public QuestionBuilder AsInactive()
    {
        _isEnabled = false;
        return this;
    }

    /// <summary>Validates inputs and constructs the final Question instance.</summary>
    /// <returns>A fully initialized Core.Models.Question object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when text or key is null/empty.</exception>
    /// <example><![CDATA[Question q = builder.WithText("Q").WithFileName("f").Build();]]></example>
    public Question Build()
    {
        Guard.Against.NullOrWhiteSpace(_text);
        Guard.Against.NullOrWhiteSpace(_key);

        return new Question(
            _isEnabled,
            _text.Trim(),
            _key,
            Category: _category);
    }
}
