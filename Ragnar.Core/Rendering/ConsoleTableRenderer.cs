namespace Ragnar.Core.Rendering;

/// <summary>
/// Default renderer that writes a table to the live console.
/// </summary>
/// <example><![CDATA[renderer.Table.AddColumns("A"); renderer.Render();]]></example>
public sealed class ConsoleTableRenderer(ConsoleTableBuilder builder) : ITableRenderer
{

    /// <summary>Exposes the underlying table builder for chain configuration.</summary>
    /// <returns>The configured <see cref="ConsoleTableBuilder"/>.</returns>
    /// <example><![CDATA[var b = renderer.Table;]]>
    /// </example>
    public ConsoleTableBuilder Table { get; } = builder;


    /// <summary>Builds the table and writes it to the console via <see cref="AnsiConsole"/>.</summary>
    /// <returns>None; renders synchronously to stdout.
    /// </returns>
    /// <example><![CDATA[renderer.Render();]]></example>
    public void Render()
    {
        var table = Table.ToTable();
        AnsiConsole.Write(table);
    }
}
