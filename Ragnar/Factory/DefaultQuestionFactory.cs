namespace Ragnar.Factory;

/// <summary>
/// Default implementation of IQuestionFactory.
/// </summary>
public class DefaultQuestionFactory : IQuestionFactory
{
    /// <summary>Creates an active (enabled) question.</summary>
    /// <param name="text">Question text.</param>
    /// <param name="key">Save filename.</param>
    /// <param name="category">Category enum.</param>
    /// <returns>New active Question.</returns>
    /// <example><![CDATA[var q = factory.CreateActive("Is this right?", "q1", Category.Refactor);]]></example>
    public Question CreateActive(string text, string key, QuestionCategory category)
        => new(true, Validate(text), Validate(key), category);

    /// <summary>Creates an inactive (disabled) question.</summary>
    /// <param name="text">Question text.</param>
    /// <param name="key">Save filename.</param>
    /// <param name="category">Category enum.</param>
    /// <returns>New inactive Question.</returns>
    /// <example><![CDATA[var q = factory.CreateInactive("Future?", "q2", Category.XML);]]></example>
    public Question CreateInactive(string text, string key, QuestionCategory category)
        => new(false, Validate(text), Validate(key), category);

    /// <summary>Validates and trims question text/key.</summary>
    /// <param name="value">Input string.</param>
    /// <param name="paramName">Caller param name.</param>
    /// <returns>Trimmed non-empty string.</returns>
    /// <example><![CDATA[var s = Validate("  test  ");]]></example>
    private static string Validate(string value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        Guard.Against.NullOrWhiteSpace(value, paramName);

        var trimmed = value.AsSpan().Trim();

        if(trimmed.Length == 0)
        {
            throw new ArgumentException("Value cannot be whitespace-only.", paramName);
        }
        return trimmed.ToString();
    }
}
