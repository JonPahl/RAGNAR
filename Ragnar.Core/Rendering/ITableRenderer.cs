namespace Ragnar.Core.Rendering;

/// <summary>
/// Base interface for rendering tabular data to the console.
/// </summary>
/// <example><![CDATA[renderer.Render();]]></example>
public interface ITableRenderer
{
    /// <summary>Builds and displays the table.</summary>
    /// <returns>The configured <see cref="ConsoleTableBuilder"/> instance.</returns>
    /// <example><![CDATA[var b = renderer.Table;]]></example>
    void Render();

    /// <summary>The underlying table being built.</summary>
    /// <example><![CDATA[renderer.Render();]]></example>
    ConsoleTableBuilder Table { get; }
}
