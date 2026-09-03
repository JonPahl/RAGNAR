namespace Ragnar.Questions;

/// <summary>Selects appropriate question filter based on input criteria.</summary>
public static class QuestionFilterSelector
{
    private static readonly ImmutableDictionary<QuestionCategory, IFilterStrategy> _strategies =
    ImmutableDictionary<QuestionCategory, IFilterStrategy>.Empty
        .Add(QuestionCategory.XML, new XmlCommentFilterStrategy());

    /// <summary>Selects appropriate filter strategy for a question category and optional size.</summary>
    /// <param name="category">The question category (e.g., XML).</param>
    /// <param name="commentLengthThreshold">Optional comment length threshold.</param>
    /// <returns>A configured <see cref="Filter"/> instance.</returns>
    /// <example><![CDATA[var filter = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 100);]]></example>
    public static Filter FindFilter(QuestionCategory category, int commentLengthThreshold = 0)
    {
        if (!_strategies.TryGetValue(category, out var strategy))
            return new();

        // Decide strategy based on size at runtime
        return commentLengthThreshold != 0 ? strategy.CreateFilter(commentLengthThreshold)
        : new XmlCommentFilterStrategy().CreateFilter(commentLengthThreshold);
    }
}
