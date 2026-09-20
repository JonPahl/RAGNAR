namespace Ragnar.Questions.Filters;

/// <summary>Defines filtering rules for a specific question category.</summary>
/// <example><![CDATA[var f = strategy.CreateFilter(50);]]></example>
public interface IFilterStrategy
{
    /// <summary>Gets the question category this strategy applies to.</summary>
    /// <returns>The supported <see cref="QuestionCategory"/> value.</returns>
    /// <example><![CDATA[QuestionCategory c = strategy.SupportedCategory;]]></example>
    QuestionCategory SupportedCategory { get; }

    /// <summary>Creates a filter predicate for the given size threshold.</summary>
    /// <param name="sizeThreshold">Minimum character-count threshold to apply.</param>
    /// <returns>A configured <see cref="Filter"/> instance.</returns>
    /// <example><![CDATA[var f = strategy.CreateFilter(50);]]></example>
    Filter CreateFilter(int sizeThreshold);
}
