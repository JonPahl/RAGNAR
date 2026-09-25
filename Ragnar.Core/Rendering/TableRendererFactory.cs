namespace Ragnar.Core.Rendering;

/// <summary>
/// Fluent factory that composes decorators around a base table renderer.
/// </summary>
/// <example>
/// <![CDATA[
/// TableRendererFactory.Create()
///     .WithColumns("cnt", "Path")
///     .WithRows(files.Select(f => new[] { f }))
///     .StyledBorder(Color.Green, "Source Files")
///     .ShowRowSeparators()
///     .Render();
/// ]]>
/// </example>
public sealed class TableRendererFactory
{
    private readonly ConsoleTableBuilder _builder = new();
    private ITableRenderer? _renderer;

    public static TableRendererFactory Create() => new();

    public TableRendererFactory WithColumns(params string[] columns)
    {
        _builder.AddColumns(columns);
        return this;
    }


    public TableRendererFactory WithRows(IEnumerable<IEnumerable<string>> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        foreach (var row in rows)
            _builder.AddRow([.. row]);
        return this;
    }

    public TableRendererFactory ShowRowSeparators()
    {
        _builder.ShowRowSeparators = true;
        return this;
    }

    public TableRendererFactory Expand()
    {
        _builder.Expand = true;
        return this;
    }

    public TableRendererFactory StyledBorder(Color color, string? header = null)
    {
        _renderer = new StyledBorderDecorator(BuildBase(), color, header);
        return this;
    }

    public TableRendererFactory WithLiveProgress()
    {
        _renderer = new LiveProgressDecorator(BuildBase(), _ => _builder.Rows.Count);
        return this;
    }

    public void Render() => (_renderer ?? BuildBase()).Render();

    private ITableRenderer BuildBase() => new ConsoleTableRenderer(_builder);
}
