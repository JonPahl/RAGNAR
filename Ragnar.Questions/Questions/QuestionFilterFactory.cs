namespace Ragnar.Questions.Questions;

/// <summary>Factory to select appropriate filter strategy based on question category.</summary>
public static class QuestionFilterFactory
{
    /// <summary>Finds appropriate filter strategy for a given question category and optional size.</summary>
    /// <param name="category">The question category.</param>
    /// <param name="size">Optional size threshold (e.g., comment length).</param>
    /// <returns>A configured <see cref="Filter"/> instance.</returns>
    public static Filter FindFilter (QuestionCategory category, int size = 0) => category switch
    {
        QuestionCategory.XML => size is not 0 ? new XmlCommentLengthFilterStrategy().CreateFilter(category) : new XmlCommentFilterStrategy().CreateFilter(category),
        _ => new Filter(),
    };
}
