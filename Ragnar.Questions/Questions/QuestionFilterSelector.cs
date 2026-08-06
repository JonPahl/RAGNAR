namespace Ragnar.Questions.Questions;

/// <summary>Factory to select appropriate filter strategy based on question category.</summary>
public static class QuestionFilterSelector
{
    private static readonly ImmutableDictionary<QuestionCategory, IFilterStrategy> Strategies =
    ImmutableDictionary<QuestionCategory, IFilterStrategy>.Empty
        .Add(QuestionCategory.XML, new XmlCommentLengthFilterStrategy());

    /// <summary>Selects appropriate filter strategy for a question category and optional size.</summary>
    /// <param name="Category">The question category (e.g., XML).</param>
    /// <param name="CommentLengthThreshold">Optional comment length threshold.</param>
    /// <returns>A configured <see cref="Filter"/> instance.</returns>
    /// <example><![CDATA[var filter = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 100);]]></example>
    public static Filter FindFilter(QuestionCategory Category, int CommentLengthThreshold = 0)
    {
        if(!Strategies.TryGetValue(Category, out var Strategy))
            return new();

        // Decide strategy based on size at runtime
        return CommentLengthThreshold != 0 ? Strategy.CreateFilter(Category, CommentLengthThreshold)
                         : new XmlCommentFilterStrategy().CreateFilter(Category, CommentLengthThreshold);
    }
}
