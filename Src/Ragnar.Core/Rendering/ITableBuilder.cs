namespace Ragnar.Core.Rendering;

/// <summary>Fluent builder for constructing Spectre.Console table structures.</summary>
/// <example><![CDATA[var t = new ConsoleTableBuilder().AddColumns("A","B").AddRow("1","2");]]></example>
public interface ITableBuilder
{
    /// <summary>Gets the number of columns in the table.</summary>
    int ColumnCount { get; set; }

    /// <summary>Gets the ordered column header labels.</summary>
    IReadOnlyList<string> Columns { get; set; }

    /// <summary>Gets or sets whether the table expands to full console width.</summary>
    bool Expand { get; set; }

    /// <summary>Gets the number of data rows in the table.</summary>
    int RowCount { get; }

    /// <summary>Gets the collection of row cell arrays.</summary>
    IReadOnlyList<string[]> Rows { get; }

    /// <summary>Gets or sets whether row separator lines are rendered.</summary>
    bool ShowRowSeparators { get; set; }

    /// <summary>Adds column headers to the table.</summary>
    /// <param name="columns">Column header labels.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example><![CDATA[builder.AddColumns("Name", "Value");]]></example>
    ConsoleTableBuilder AddColumns(params string[] columns);

    /// <summary>Adds a data row to the table.</summary>
    /// <param name="cells">Cell values for the row.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example><![CDATA[builder.AddRow("alpha", "42");]]></example>
    ConsoleTableBuilder AddRow(KeyValuePair<int, List<TableCell>> row, params string[] cells);

    /// <summary>Removes all columns and rows from the table.</summary>
    /// <returns>The builder for chaining.</returns>
    /// <example><![CDATA[builder.Clear();]]></example>
    ConsoleTableBuilder Clear();

    /// <summary>Converts the built configuration into a Spectre.Console Table.</summary>
    /// <returns>The rendered Table instance.</returns>
    /// <example><![CDATA[AnsiConsole.Write(builder.ToTable());]]></example>
    Table ToTable();
}
