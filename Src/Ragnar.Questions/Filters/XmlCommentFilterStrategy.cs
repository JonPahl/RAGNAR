namespace Ragnar.Questions.Filters;

/// <summary>Filters XML comments based on size and content rules.</summary>
/// <remarks>Excludes test-related categories and correct length comments below threshold.</remarks>
/// <example><![CDATA[var f = strategy.CreateFilter(50);]]></example>

public sealed class XmlCommentFilterStrategy(Serilog.ILogger logger)
    : IFilterStrategy
{
    /// <summary>Gets the supported question category for this filtering strategy.</summary>
    /// <value>Always returns QuestionCategory.XML.</value>
    /// <example><![CDATA[QuestionCategory cat = strategy.SupportedCategory;]]></example>
    public QuestionCategory SupportedCategory => QuestionCategory.XML;

    /// <summary>Creates a filter to exclude non-empty XML comments based on size.</summary>
    /// <remarks>A filter that excludes comments longer than the specified threshold.</remarks>
    /// <param name="sizeThreshold">The minimum element size threshold.</param>
    /// <returns>The configured filter instance.</returns>
    /// <example><![CDATA[var f = strategy.CreateFilter(50);]]></example>
    public Filter CreateFilter(int sizeThreshold)
    {
        logger.Debug("Creating XML comment filter with size threshold {Threshold}", sizeThreshold);

        var filter = new QdrantFilterBuilder()
            .AddRange("CommentLength", sizeThreshold)
            .AddExceptKeywords("Category", "Test", "Tests", "Testing")
            .Build();

        logger.Debug("XML comment filter built: {MustCount} must, {MustNotCount} mustNot conditions",
            filter.Must?.Count ?? 0, filter.MustNot?.Count ?? 0);
        return filter;
    }
}
