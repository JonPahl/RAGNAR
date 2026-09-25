namespace Ragnar.Core.Rendering.Decorators;

/// <summary>
/// Decorator that applies a colored border and optional header to the table.
/// </summary>
public sealed class StyledBorderDecorator(
    ITableRenderer inner,
    Color borderColor,
    string? header = null) : ITableRenderer
{
    public ConsoleTableBuilder Table => inner.Table;

    public void Render()
    {
        var table = Table.ToTable()
            .BorderColor(borderColor)
            .BorderStyle(new Style { Foreground = borderColor });

        if (header is not null)
            table.ShowHeaders();

        AnsiConsole.Write(table);
    }
}
