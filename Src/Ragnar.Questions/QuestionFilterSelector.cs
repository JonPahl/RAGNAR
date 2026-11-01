namespace Ragnar.Questions;

/// <summary>Initializes the selector with a collection of filter strategies.</summary>
/// <param name="strategies">Strategies keyed by supported question category.</param>
/// <example><![CDATA[var f = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 100);]]></example>
public sealed class QuestionFilterSelector(Serilog.ILogger logger, IEnumerable<IFilterStrategy> strategies)
{
    private readonly Dictionary<QuestionCategory, IFilterStrategy> _strategies = strategies.ToDictionary(s => s.SupportedCategory);

    /// <summary>Finds and creates a Qdrant filter for the given category and threshold.</summary>
    /// <param name = "category">The question category to filter on.</param>
    /// <param name = "commentLengthThreshold">Minimum comment length for range filtering.</param>
    /// <returns>A Qdrant Filter instance, or an empty Filter if no strategy matches.</returns>
    /// <example><![CDATA[var f = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 100);]]></example>
    public Filter FindFilter(QuestionCategory category, int commentLengthThreshold = 0)
    {
        _strategies.TryAdd(QuestionCategory.XML, new XmlCommentFilterStrategy(logger));

        logger.Debug("Finding filter for category {Category}, threshold {Threshold}", category, commentLengthThreshold);

        var filter = _strategies.TryGetValue(category, out var strategy)
    ? strategy.CreateFilter(commentLengthThreshold)
    : new Filter();
        logger.Debug("Filter resolved for {Category}: {HasConditions} conditions", category, filter.Must?.Count > 0 || filter.MustNot?.Count > 0);
        return filter;
    }
}
