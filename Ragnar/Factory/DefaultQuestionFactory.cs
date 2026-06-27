using System.Runtime.CompilerServices;

using Ragnar.Questions.Interface;

namespace Ragnar.Factory;
/// <summary>
/// Default implementation of IQuestionFactory.
/// </summary>
public class DefaultQuestionFactory
    : IQuestionFactory
{
    /// <summary>Creates an active question.</summary>
    /// <param name="text">Text of question.</param>
    /// <param name="key">Save file name.</param>
    /// <param name="category">Question Category.</param>
    /// <returns>Newly created ACTIVE question.</returns>
    /// <example>
    /// <![CDATA[var q = factory.CreateActive("Is this correct?", "correct", QuestionCategory.Refactor);]]>
    /// </example>
    public Question CreateActive(string text, string key, QuestionCategory category)
        => new(true, Validate(text), Validate(key), category);

    /// <summary>Creates an inactive question.</summary>
    /// <param name="text">Text of question.</param>
    /// <param name="key">Save file name.</param>
    /// <param name="category">Question Category.</param>
    /// <remarks>Allow to turn a question off if not needed for current execution.</remarks>
    /// <returns>Newly created INACTIVE question that will not be asked.</returns>
    /// <example><![CDATA[var q = factory.CreateInactive("Future?", "future", QuestionCategory.XML);]]></example>
    public Question CreateInactive(string text, string key, QuestionCategory category)
        => new(false, Validate(text), Validate(key), category);

    private static string Validate(string value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        try
        {
            Guard.Against.NullOrWhiteSpace(value, paramName);

            var trimmed = value.AsSpan().Trim();

            return trimmed.Length == 0
                ? throw new ArgumentException("Value cannot be whitespace-only.", paramName)
                : trimmed.ToString();
        }
        catch (ArgumentNullException ex)
        {
            throw new ArgumentException(ex.Message, ex);
        }
    }
}
