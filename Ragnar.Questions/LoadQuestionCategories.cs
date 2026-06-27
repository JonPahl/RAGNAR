using System.Collections.Immutable;

namespace Ragnar.Questions;
/// <summary>
/// Build list of enum options to Hashset.
/// </summary>
public static class LoadQuestionCategories
{
    /// <summary>Returns all values of the QuestionCategory enum.</summary>
    /// <returns>HashSet of all QuestionCategory values.</returns>
    /// <example><![CDATA[var categories = LoadQuestionCategories.All();]]></example>
    public static ImmutableHashSet<QuestionCategory> All() => [.. Enum.GetValues<QuestionCategory>()];
}
