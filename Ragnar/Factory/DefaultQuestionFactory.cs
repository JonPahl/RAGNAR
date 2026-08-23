namespace Ragnar.Factory;

/// <summary>Initializes a new instance of the default question factory.</summary>
public class DefaultQuestionFactory
    : IQuestionFactory
{
    /// <summary>Creates an active question.</summary>
    /// <param name="Text">Text of question.</param>
    /// <param name="Key">Save file name.</param>
    /// <param name="Category">Question Category.</param>
    /// <returns>Newly created ACTIVE question.</returns>
    /// <example>
    /// <![CDATA[var q = factory.CreateActive("Is this correct?", "correct", QuestionCategory.Refactor);]]>
    /// </example>
    public Question CreateActive(string Text, string Key, QuestionCategory Category)
        => new(true, Validate(Text), Validate(Key), Category);

    /// <summary>Creates an inactive question.</summary>
    /// <param name="Text">Text of question.</param>
    /// <param name="Key">Save file name.</param>
    /// <param name="Category">Question Category.</param>
    /// <remarks>Allow to turn a question off if not needed for current execution.</remarks>
    /// <returns>Newly created INACTIVE question that will not be asked.</returns>
    /// <example><![CDATA[var q = factory.CreateInactive("Future?", "future", QuestionCategory.XML);]]></example>
    public Question CreateInactive(string Text, string Key, QuestionCategory Category)
        => new(false, Validate(Text), Validate(Key), Category);

    /// <summary>Validates and trims a string value for question data.</summary>
    /// <param name = "Value"> Input string to validate and trim.</param>
    /// <param name = "ParamName"> Name of the parameter for error reporting.</param>
    /// <returns>The trimmed, non-empty string value.</returns>
    private static string Validate(string Value, [CallerArgumentExpression(nameof(Value))] string? ParamName = null)
    {
        try
        {
            Guard.Against.NullOrWhiteSpace(Value, ParamName);

            var trimmed = Value.AsSpan().Trim();

            return trimmed.Length == 0
                ? throw new ArgumentException("Value cannot be whitespace-only.", ParamName)
                : trimmed.ToString();
        }
        catch (ArgumentNullException ex)
        {
            throw new ArgumentException(ex.Message, ex);
        }
    }
}
