namespace Ragnar.Core.Rendering;

/// <summary>
/// Fluent factory that composes decorators around a base table renderer.
/// </summary>
/// <example>
/// <![CDATA[ TableRendererFactory.Create()
///     .WithColumns("cnt", "Path")
///     .WithRows(files.Select(f => new[] { f }))
///     .WithStyledBorder(Color.Green, "Source Files")
///     .WithRowSeparators()
///     .Render(); ]]>
/// </example>
public sealed class TableRendererFactory()
{
    private readonly ConsoleTableBuilder _builder = new();

    private readonly List<Func<ITableRenderer, ITableRenderer>> _decorators = [];

    /// <summary>Creates a new TableRendererFactory instance for fluent table construction.</summary>
    /// <returns>A fresh factory instance ready for column and row configuration.</returns>
    /// <example><![CDATA[TableRendererFactory.Create().WithColumns("A").Render();]]></example>
    public static TableRendererFactory Create() => new();

    /// <summary>Adds one or more column headers to the table being built.</summary>
    /// <param name="columns">Column header names to display in order.</param>
    /// <returns>This factory instance for further configuration.</returns>
    /// <example><![CDATA[factory.WithColumns("Name", "Size", "Date");]]></example>
    public TableRendererFactory WithColumns(params string[] columns)
    {
        _builder.AddColumns(columns);
        return this;
    }

    /// <summary>Adds data rows to the table, each row matching the column count.</summary>
    /// <param name="rows">Enumerable of row value collections to render.</param>
    /// <returns>This factory instance for further configuration.</returns>
    /// <example><![CDATA[factory.WithRows(files.Select(f => new[[]] { f.Name }));]]></example>
    public TableRendererFactory WithRows(IEnumerable<IEnumerable<string>> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var i = 0;
        foreach (var row in rows)
        {
            _builder.AddRow(new KeyValuePair<int, List<TableCell>>(i, [.. row.Select(s => new TableCell(s))]), [.. row]);
            i++;
        }

        return this;
    }

    /// <summary>Enables horizontal separator lines between each data row.</summary>
    /// <returns>This factory instance with row separators enabled.</returns>
    /// <example><![CDATA[factory.WithRowSeparators();]]></example>
    public TableRendererFactory WithRowSeparators()
    {
        _builder.ShowRowSeparators = true;
        return this;
    }

    /// <summary>Configures the table to expand to the full console width.</summary>
    /// <returns>This factory instance with full-width expansion enabled.</returns>
    /// <example><![CDATA[factory.WithFullWidth();]]></example>
    public TableRendererFactory WithFullWidth()
    {
        _builder.Expand = true;
        return this;
    }

    /// <summary>Applies a colored styled border decorator with an optional header label.</summary>
    /// <param name="color">Border color for the styled table frame.</param>
    /// <param name="header">Optional title text displayed above the table border.</param>
    /// <returns>This factory instance with the styled border decorator applied.</returns>
    /// <example><![CDATA[factory.WithStyledBorder(Color.Green, "Results");]]></example>
    public TableRendererFactory WithStyledBorder(Color color, string? header = null)
    {
        _decorators.Add(inner => new StyledBorderDecorator(inner, color, header));

        return this;
    }

    /// <summary>Applies a live progress decorator that updates the row count in real time.</summary>
    /// <returns>This factory instance with the live progress decorator applied.</returns>
    /// <example><![CDATA[factory.WithLiveProgress();]]>
    /// </example>
    public TableRendererFactory WithLiveProgress()
    {
        _decorators.Add(inner => new LiveProgressDecorator(inner, _ => _builder.Rows.Count));

        return this;
    }

    /// <summary>Renders the fully configured table to the console output stream.</summary>
    /// <returns>None; writes synchronously to the active console.</returns>
    /// <example><![CDATA[factory.Render();]]></example>
    public void Render()
    {
        var result = CreateBaseRenderer();
        foreach (var decorate in _decorators)
            result = decorate(result);

        result.Render();
    }

    /// <summary>Creates the base ConsoleTableRenderer from the accumulated builder state.</summary>
    /// <returns>A new ConsoleTableRenderer bound to the current builder configuration.</returns>
    /// <example><![CDATA[// Called internally by Render and decorator methods.]]></example>
    private ITableRenderer CreateBaseRenderer() => new ConsoleTableRenderer(_builder);
}
