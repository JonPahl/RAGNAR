namespace Ragnar.Questions;

/// <summary>Selects appropriate question filter based on input criteria.</summary>
/// <example><![CDATA[var f = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 100);]]></example>
public sealed class QuestionFilterSelector
{
    private readonly Dictionary<QuestionCategory, IFilterStrategy> _strategies;

    public QuestionFilterSelector(IEnumerable<IFilterStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.SupportedCategory);
        _strategies.TryAdd(QuestionCategory.XML, new XmlCommentFilterStrategy()); // sensible default
    }

    public Filter FindFilter(QuestionCategory category, int commentLengthThreshold = 0)
        => _strategies.TryGetValue(category, out var strategy)
            ? strategy.CreateFilter(commentLengthThreshold)
            : new Filter();
}
