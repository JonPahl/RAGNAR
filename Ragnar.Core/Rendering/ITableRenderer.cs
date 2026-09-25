namespace Ragnar.Core.Rendering;

/// <summary>Renders a configured table to the console output.</summary>
/// <example><![CDATA[renderer.Render();]]></example>
public interface ITableRenderer
{
    /// <summary>Gets the underlying table builder for chain configuration.</summary>
    /// <returns>The configured table builder instance.</returns>
    /// <example><![CDATA[var b = renderer.Table;]]></example>
    void Render();

    /// <summary>Builds the table and writes it to the console via AnsiConsole.</summary>
    /// <returns>None; renders synchronously to stdout.</returns>
    /// <example><![CDATA[renderer.Render();]]></example>
    ConsoleTableBuilder Table { get; }
}
