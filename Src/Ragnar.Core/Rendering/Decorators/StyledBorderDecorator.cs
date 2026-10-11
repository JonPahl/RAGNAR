namespace Ragnar.Core.Rendering.Decorators;

/// <summary>
/// Decorator that applies a colored border and optional header to the table.
/// </summary>
/// <summary>Initializes a new border decorator wrapping an inner table renderer.</summary>
/// <param name="inner">The next renderer in the decoration chain.</param>
/// <param name="borderColor">The ANSI color applied to the table border.</param>
/// <param name="header">Optional header text displayed above the table.</param>
public sealed class StyledBorderDecorator(
    ITableRenderer inner,
    Color borderColor,
    string? header = null) : ITableRenderer
{
    /// <summary>Gets the underlying table builder for further configuration.</summary>
    /// <returns>The underlying console-table builder for fluent configuration.</returns>
    /// <example><![CDATA[ConsoleTableBuilder t = decorator.Table;]]></example>
    public ConsoleTableBuilder Table => inner.Table;

    /// <summary>Renders the decorated table with border color and optional header to the console.</summary>
    /// <example><![CDATA[decorator.Render();]]></example>
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
