namespace Ragnar.Questions.Filters;

/// <summary>Defines filtering rules for vector store queries by category and size.</summary>
/// <example><![CDATA[var f = strategy.CreateFilter(50);]]></example>
public interface IFilterStrategy
{
    /// <summary>Gets the question category this strategy supports.</summary>
    /// <returns>The supported <see cref="QuestionCategory"/> value.</returns>
    /// <example><![CDATA[QuestionCategory c = strategy.SupportedCategory;]]></example>
    QuestionCategory SupportedCategory { get; }

    /// <summary>Creates a Qdrant filter based on the given size threshold.</summary>
    /// <param name="sizeThreshold">Minimum element size threshold for filtering.</param>
    /// <returns>A configured Qdrant Filter instance.</returns>
    /// <example><![CDATA[var f = strategy.CreateFilter(50);]]></example>
    Filter CreateFilter(int sizeThreshold);
}
