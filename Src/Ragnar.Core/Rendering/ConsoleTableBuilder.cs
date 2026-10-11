namespace Ragnar.Core.Rendering;

/// <summary>
/// Mutable builder for a Spectre.Console table.
/// </summary>
/// <example><![CDATA[var t = new ConsoleTableBuilder().AddColumns("A","B").AddRow("1","2").ToTable();]]>
/// </example>
public sealed class ConsoleTableBuilder
    : ITableBuilder
{
    /// <summary>Cached expected cell count per row, locked on the first <c>AddRow</c> call.</summary>
    private int? _expectedCellCount;

    private bool _columnsLocked;

    /// <summary>Gets or sets whether horizontal lines appear between table rows.</summary>
    /// <example><![CDATA[builder.WithRowSeparators = true;]]></example>
    public bool ShowRowSeparators { get; set; }

    /// <summary>Gets or sets whether the table expands to fill the console width.</summary>
    /// <example><![CDATA[builder.WithFullWidth = true;]]>
    /// </example>
    public bool Expand { get; set; }

    /// <summary>Adds one or more column headers to the table definition.</summary>
    /// <param name = "columns">Column header names to append to the table.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example><![CDATA[builder.AddColumns("Name", "Value");]]></example>
    /// <exception cref="ArgumentException"></exception>
    public ConsoleTableBuilder AddColumns(params string[] columns)
    {
        Guard.Against.NullOrEmpty(columns);

        if (_columnsLocked)
            throw new ArgumentException("Columns are already defined. Call Clear() first.");

        foreach (var col in columns)
        {
            Guard.Against.NullOrWhiteSpace(col);
            if (_columns.Contains(col, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Duplicate column '{col}'.", nameof(columns));
            }
        }

        _columns.AddRange(columns);
        _columnsLocked = true;

        return this;
    }

    /// <summary>Appends a data row whose cells match the previously defined column count.</summary>
    /// <param name = "cells">Cell values for the new row; count must match columns.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <example>
    /// <![CDATA[builder.AddRow("Hello", "World");]]>
    /// </example>
    /// <exception cref="ArgumentException"></exception>
    public ConsoleTableBuilder AddRow(KeyValuePair<int, List<TableCell>> row, params string[] cells)
    {
        Guard.Against.NullOrEmpty(cells);

        _expectedCellCount ??= cells.Length;
        if (cells.Length != _expectedCellCount)
        {
            throw new ArgumentException(
                $"Row has {cells.Length} cells but expected {_expectedCellCount}.",
                nameof(cells));
        }

        _rows.Add(cells);
        return this;
    }

    /// <summary>Removes all previously added rows and columns.</summary>
    /// <example>
    /// <![CDATA[[builder.AddColumns("A","B").AddRow("1","2").Clear();]]>
    /// </example>
    /// <returns>The same builder instance for fluent chaining.</returns>
    public ConsoleTableBuilder Clear()
    {
        _columns.Clear();
        _rows.Clear();
        _expectedCellCount = null;
        _columnsLocked = false;
        return this;
    }

    /// <summary>Read-only snapshot of column headers.</summary>
    /// <example><![CDATA[List<string> cols = builder.Columns;]]></example>
    private readonly List<string> _columns = [];

    /// <summary>Read-only snapshot of row data (immutable arrays).</summary>
    /// <example><![CDATA[List<string[]> rows = builder.Rows;]]></example>
    private readonly List<string[]> _rows = [];

    /// <summary>Number of data rows defined.
    /// </summary>
    /// <example><![CDATA[int count = builder.RowCount;]]></example>
    public int RowCount => _rows.Count;

    public IReadOnlyList<string> Columns => _columns;

    public IReadOnlyList<string[]> Rows => _rows;

    public int ColumnCount { get; set; }
    IReadOnlyList<string> ITableBuilder.Columns { get => Columns; set => throw new NotImplementedException(); }

    /// <summary>Materializes a Spectre.Console <see cref="Table"/> from accumulated state.</summary>
    /// <exception cref="InvalidOperationException">Thrown when no columns or rows have been configured.</exception>
    /// <example>
    /// <![CDATA[Table t = builder
    /// .AddColumns("A").AddRow("1").ToTable(); ]]>
    /// </example>
    /// <returns>A configured Spectre.Console Table instance ready for rendering.
    /// </returns>
    public Table ToTable()
    {
        if (_columns.Count == 0 && _rows.Count == 0)
        {
            throw new InvalidOperationException(
                "No columns or rows have been configured.");
        }

        var table = new Table();
        if (Expand) table.Expand();
        if (ShowRowSeparators) table.ShowRowSeparators();

        foreach (var col in _columns) table.AddColumn(col);
        foreach (var row in _rows) table.AddRow(row);

        return table;
    }

}
