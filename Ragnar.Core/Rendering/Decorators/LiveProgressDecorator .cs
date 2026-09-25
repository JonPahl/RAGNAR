namespace Ragnar.Core.Rendering.Decorators;

/// <summary>
/// Decorator that renders the table inside a Spectre.Console Live context,
/// refreshing after every row is added (for progressive / animated display).
/// </summary>
/// <param name="inner">The renderer to decorate.</param>
/// <param name="rowFactory">
///   A delegate that, given the current row list, appends one row and returns
///   the total row count (used as the progress max).
/// </param>
public sealed class LiveProgressDecorator(ITableRenderer inner, Func<List<string>, int> rowFactory) : ITableRenderer
{
    /// <summary>Forwards to the inner renderer's table builder instance.</summary>
    /// <returns>The underlying <see cref="ConsoleTableBuilder"/>.</returns>
    /// <example><![CDATA[var b = decorator.Table;]]></example>
    public ConsoleTableBuilder Table => inner.Table;

    /// <summary>Renders the table inside a Spectre.Console Live context with per-row refresh.</summary>
    /// <returns>None; renders synchronously to the console.</returns>
    /// <example><![CDATA[decorator.Render();]]></example>
    public void Render()
    {
        var table = Table.ToTable();
        var total = rowFactory(null);

        AnsiConsole.Live(table).Start(ctx =>
        {
            for (var i = 0; i < Table.Rows.Count; i++)
            {
                ctx.Refresh();
            }
        });
    }
}
