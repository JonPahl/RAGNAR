namespace Ragnar.Core.Rendering;

/// <summary>Fluent builder for constructing Spectre.Console table structures.</summary>
/// <example><![CDATA[var t = new ConsoleTableBuilder().AddColumns("A","B").AddRow("1","2");]]></example>
public interface ITableBuilder
{
    List<string> Columns { get; }
    bool Expand { get; set; }
    List<string[]> Rows { get; }
    bool ShowRowSeparators { get; set; }

    /// <summary>Adds column headers, rejecting duplicates and re-definition.</summary>
    /// <param name="columns">Array of column header names to add.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example><![CDATA[builder.AddColumns("Name", "Value");]]></example>
    ConsoleTableBuilder AddColumns(params string[] columns);

    /// <summary>Adds a data row, enforcing consistent cell count across rows.</summary>
    /// <param name="cells">Array of cell values matching the column count.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example>
    /// <![CDATA[builder.AddRow("1", "Hello");]]>
    /// </example>
    ConsoleTableBuilder AddRow(params string[] cells);
    Table ToTable();
}
