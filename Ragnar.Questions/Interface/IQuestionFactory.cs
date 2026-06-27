namespace Ragnar.Questions.Interface;
/// <summary>
/// A factory for creating Question instances.
/// </summary>
public interface IQuestionFactory
{
    /// <summary>Creates an active question.</summary>
    /// <param name="text">Text of question.</param>
    /// <param name="key">Save file name.</param>
    /// <param name="category">Question Category.</param>
    /// <returns>newly created ACTIVE question.</returns>
    Question CreateActive(string text, string key, QuestionCategory category);

    /// <summary>Creates an inactive question.</summary>
    /// <param name="text">Text of question.</param>
    /// <param name="key">Save file name.</param>
    /// <param name="category">Question Category.</param>
    /// <remarks>Allow to turn a question off if not needed for current execution.</remarks>
    /// <returns>newly created INACTIVE question that will not be asked.</returns>
    Question CreateInactive(string text, string key, QuestionCategory category);
}
