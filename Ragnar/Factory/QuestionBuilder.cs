namespace Ragnar.Factory;

/// <summary>Initializes a new instance of the default question factory.</summary>
public class QuestionBuilder : IQuestionBuilder
{
    private string? _text;
    private string? _key;

    private QuestionCategory _category = QuestionCategory.Other;

    private bool _isEnabled = true;

    public QuestionBuilder WithText(string text)
    {
        _text = text;
        return this;
    }
    public QuestionBuilder WithFileName(string key)
    {
        _key = key;
        return this;
    }

    public QuestionBuilder SetCategory(QuestionCategory? category)
    {
        if (category is not null)
            _category = category.Value;
        return this;
    }

    /// <summary>
    /// Set new question as active.
    /// </summary>
    /// <returns></returns>
    public QuestionBuilder AsActive()
    {
        _isEnabled = true;
        return this;
    }

    /// <summary>
    /// Set new question as inActive.
    /// </summary>
    /// <returns></returns>
    public QuestionBuilder AsInactive()
    {
        _isEnabled = false;
        return this;
    }

    public Core.Model.Question Build()
    {
        Guard.Against.NullOrWhiteSpace(_text, nameof(_text));
        Guard.Against.NullOrWhiteSpace(_key, nameof(_key));

        return new Core.Model.Question(
            _isEnabled,
            _text.Trim(),
            _key,
            Category: _category);
    }
}
