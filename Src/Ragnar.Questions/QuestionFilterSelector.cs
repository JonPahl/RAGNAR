namespace Ragnar.Questions;

/// <summary>Initializes the selector with a collection of filter strategies.</summary>
/// <example><![CDATA[var f = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 100);]]></example>
/// <remarks>Initializes the selector with a logger and a collection of filter strategies.</remarks>
/// <param name="strategies">Strategies keyed by supported question category.</param>
/// <example><![CDATA[var sel = new QuestionFilterSelector(log, strategies);]]></example>
public sealed class QuestionFilterSelector(IEnumerable<IFilterStrategy> strategies)
{
    /// <summary>Map of supported categories to their filter strategy instances.</summary>
    private readonly Dictionary<QuestionCategory, IFilterStrategy> _strategies = strategies.ToDictionary(s => s.SupportedCategory);

    /// <summary>Returns a Qdrant Filter built by the strategy registered for the given category.</summary>
    /// <param name="category">The question category to look up.</param>
    /// <param name="commentLengthThreshold">Optional minimum comment-length bound for the filter.</param>
    /// <returns>The configured Qdrant Filter instance.</returns>
    /// <example><![CDATA[var f = selector.FindFilter(QuestionCategory.XML, 100);]]></example>
    public Filter FindFilter(
        QuestionCategory category,
        int commentLengthThreshold = 0)
    {
        if (!_strategies.TryGetValue(category, out var strategy))
        {
            throw new NotSupportedException(
                $"No IFilterStrategy registered for '{category}'. " +
                $"Registered: {string.Join(", ", _strategies.Keys)}");
        }

        return strategy.CreateFilter(commentLengthThreshold);
    }
}
