namespace Ragnar.Questions;

/// <summary>Initializes the selector with a collection of filter strategies.</summary>
/// <param name="strategies">Strategies keyed by supported question category.</param>
/// <example><![CDATA[var f = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 100);]]></example>
public sealed class QuestionFilterSelector
{
    private readonly Dictionary<QuestionCategory, IFilterStrategy> _strategies;
    private readonly Serilog.ILogger _logger;

    public QuestionFilterSelector(Serilog.ILogger logger, IEnumerable<IFilterStrategy> strategies)
    {
        _logger = logger;
        _strategies = strategies.ToDictionary(s => s.SupportedCategory);

        // Register built-in strategies ONCE, at construction time.
        if (!_strategies.ContainsKey(QuestionCategory.XML))
            _strategies[QuestionCategory.XML] = new XmlCommentFilterStrategy(logger);
    }

    public Filter FindFilter(QuestionCategory category, int commentLengthThreshold = 0)
    {
        _logger.Debug("Finding filter for {Category}, threshold {Threshold}", category, commentLengthThreshold);

        var filter = _strategies.TryGetValue(category, out var strategy)
            ? strategy.CreateFilter(commentLengthThreshold)
            : new Filter();

        _logger.Debug("Filter resolved for {Category}: {MustCount} must, {MustNotCount} mustNot",
                      category, filter.Must?.Count ?? 0, filter.MustNot?.Count ?? 0);
        return filter;
    }
}
