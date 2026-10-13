namespace Ragnar.Core.Rendering.Decorators;

/// <summary>Initializes a new row-numbering decorator wrapping an inner renderer.</summary>
/// <param name="inner">The next renderer in the decoration chain.</param>
/// <param name="columnHeader">Header label for the index column; defaults to "#".</param>
public sealed class NumberedRowDecorator(
    ITableRenderer inner,
    string columnHeader = "#")
    : ITableRenderer
{
    /// <summary>Gets the underlying table builder for further configuration.</summary>
    /// <returns>The underlying console-table builder for fluent configuration.</returns>
    /// <example><![CDATA[ConsoleTableBuilder t = decorator.Table;]]></example>
    public ConsoleTableBuilder Table => inner.Table;

    private bool _numbered;

    /// <summary>Renders the wrapped table with a numbered index column prepended to the header.</summary>
    /// <example><![CDATA[decorator.Render();]]>
    /// </example>
    public void Render()
    {
        if (_numbered)
        {
            inner.Render();
            return;
        }
        _numbered = true;

        var table = inner.Table;

        // 1. Ensure the index column exists
        if (!table.Columns.Contains(columnHeader, StringComparer.OrdinalIgnoreCase))
            table.AddColumns(columnHeader);

        // 2. Prepend a 1-based index to every existing row
        var existing = table.Rows.Select(r => r.ToArray()).ToList();

        for (var i = 0; i < existing.Count; i++)
        {
            var indexed = new string[table.Columns.Count];
            indexed[0] = (i + 1).ToString();
            for (var c = 0; c < existing[i].Length; c++)
            {
                indexed[c + 1] = existing[i][c];
            }

            KeyValuePair<int, List<TableCell>> row = new(i, [.. indexed.Select(cell => new TableCell(cell))]);

            table.AddRow(row);
        }

        // 3. Delegate final rendering (borders, colors, etc.) to inner
        inner.Render();
    }
}
