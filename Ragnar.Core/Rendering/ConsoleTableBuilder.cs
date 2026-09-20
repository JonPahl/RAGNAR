namespace Ragnar.Core.Rendering;

/// <summary>
/// Mutable builder for a Spectre.Console table.
/// </summary>
/// <example><![CDATA[var t = new ConsoleTableBuilder().AddColumns("A","B").AddRow("1","2").ToTable();]]></example>
public sealed class ConsoleTableBuilder : ITableBuilder
{
    private int? _expectedCellCount;


    /// <summary>Gets or sets whether horizontal lines appear between table rows.</summary>
    /// <example><![CDATA[builder.ShowRowSeparators = true;]]></example>
    public bool ShowRowSeparators { get; set; }


    /// <summary>Gets or sets whether the table expands to fill the console width.</summary>
    /// <example><![CDATA[builder.Expand = true;]]></example>
    public bool Expand { get; set; }

    /// <inheritdoc/>
    public ConsoleTableBuilder AddColumns(params string[] columns)
    {
        Guard.Against.Null(columns);
        Guard.Against.NullOrEmpty(columns);

        // Prevent accidental re-definition
        if (Columns.Count > 0)
        {
            throw new InvalidOperationException(
                "Columns are already defined. Call ClearColumns() first to reconfigure.");
        }

        foreach (var col in columns)
        {
            Guard.Against.NullOrWhiteSpace(col, nameof(columns));
            if (Columns.Contains(col, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException($"Duplicate column '{col}'.", nameof(columns));
        }

        Columns.AddRange(columns);
        return this;
    }

    /// <inheritdoc/>
    public ConsoleTableBuilder AddRow(params string[] cells)
    {
        Guard.Against.Null(cells);
        Guard.Against.NullOrEmpty(cells);

        // Lock cell count on first row
        if (_expectedCellCount is null)
        {
            _expectedCellCount = cells.Length;
        }
        else if (cells.Length != _expectedCellCount)
        {
            throw new ArgumentException(
                        $"Row has {cells.Length} cells but expected {_expectedCellCount} (matching column count).",
                        nameof(cells));
        }

        Rows.Add(cells);
        return this;
    }

    /// <summary>Removes all previously added rows and columns.</summary>
    /// <example>
    /// <![CDATA[[builder.AddColumns("A","B").AddRow("1","2").Clear();]]>
    /// </example>
    /// <returns>The same builder instance for fluent chaining.</returns>
    public ConsoleTableBuilder Clear()
    {
        Columns.Clear();
        Rows.Clear();
        _expectedCellCount = null;
        return this;
    }

    /// <summary>Read-only snapshot of column headers.</summary>
    /// <example><![CDATA[List<string> cols = builder.Columns;]]></example>
    public List<string> Columns { get; set; } = [];

    /// <summary>Read-only snapshot of row data (immutable arrays).</summary>
    /// <example><![CDATA[List<string[]> rows = builder.Rows;]]></example>
    public List<string[]> Rows { get; set; } = [];

    /// <summary>Number of columns defined.</summary>
    /// <example><![CDATA[int count = builder.ColumnCount;]]></example>
    public int ColumnCount => Columns.Count;

    /// <summary>Number of data rows defined.</summary>
    /// <example><![CDATA[int count = builder.RowCount;]]></example>
    public int RowCount => Rows.Count;

    /// <summary>Materialises a Spectre.Console <see cref="Spectre.Console.Table"/> from accumulated state.</summary>
    /// <exception cref="InvalidOperationException">Thrown when no columns or rows have been configured.</exception>
    /// <example>
    /// <![CDATA[Table t = builder
    /// .AddColumns("A").AddRow("1").ToTable(); ]]>
    /// </example>
    /// <returns>A configured Spectre.Console Table instance ready for rendering.</returns>
    public Table ToTable()
    {
        var table = new Table();

        if (Expand)
            table.Expand();

        if (ShowRowSeparators)
            table.ShowRowSeparators();

        foreach (var col in Columns)
            table.AddColumn(col);

        foreach (var row in Rows)
            table.AddRow(row);

        return table;
    }
}
