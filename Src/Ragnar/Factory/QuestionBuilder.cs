namespace Ragnar.Factory;

/// <summary>Initializes a new instance of the default question factory.</summary>
public class QuestionBuilder : IQuestionBuilder
{
    /// <summary>Stores the question text to be assigned during Build().</summary>
    private string _text = string.Empty;

    /// <summary>Stores the source-file key identifier for the question.</summary>
    private string _fileName = string.Empty;

    /// <summary>The classification category; defaults to Other.</summary>
    private QuestionCategory _category;

    /// <summary>Whether the question is active; defaults to true.</summary>
    private bool _isActive = true;

    private Filter? _filter = null;

    /// <summary>Sets the question text and returns the builder for chaining.</summary>
    /// <param name="text">The question or prompt text to assign.</param>
    /// <returns>This builder instance for further configuration.</returns>
    /// <example><![CDATA[builder.SetText("Explain this code");]]></example>
    public IQuestionBuilder SetText(string text)
    {
        Guard.Against.NullOrWhiteSpace(text);
        _text = text;
        return this;
    }

    /// <summary>Assigns a source-file key identifier to the question.</summary>
    /// <param name="key">The filename or unique identifier for the question.</param>
    /// <returns>This builder instance for further configuration.</returns>
    /// <example><![CDATA[builder.SetFileName("Program.cs");]]></example>
    public IQuestionBuilder SetFileName(string key)
    {
        Guard.Against.NullOrWhiteSpace(key);
        _fileName = key;
        return this;
    }

    /// <summary>Sets the question category; pass null to retain the current value.</summary>
    /// <param name="category">The target category, or null to leave unchanged.</param>
    /// <returns>This builder instance for further configuration.</returns>
    /// <example><![CDATA[builder.SetCategory(QuestionCategory.Performance);]]></example>
    public IQuestionBuilder SetCategory(QuestionCategory category)
    {
        _category = category;
        return this;
    }

    public IQuestionBuilder SetFilter(Filter? filter = null)
    {
        _filter = filter;
        return this;
    }

    public IQuestionBuilder SetActive(bool value = true)
    {
        _isActive = value;
        return this;
    }

    /// <summary>Validates inputs and constructs the final Question instance.</summary>
    /// <returns>A fully initialized Core.Models.Question object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when text or key is null/empty.</exception>
    /// <example><![CDATA[Question q = builder.SetText("Q").SetFileName("f").Build();]]></example>
    public Question Build()
    {
        if (string.IsNullOrWhiteSpace(_text))
            throw new InvalidOperationException("Question text was not set. Call SetText() first.");
        if (string.IsNullOrWhiteSpace(_fileName))
            throw new InvalidOperationException("Question FileName was not set. Call SetFileName() first.");

        return new Question(_isActive, _text, _fileName, _category, _filter);
    }
}
