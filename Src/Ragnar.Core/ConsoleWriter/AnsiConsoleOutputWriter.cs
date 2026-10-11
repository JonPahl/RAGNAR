namespace Ragnar.Core.ConsoleWriter;

/// <summary>
/// output writer to wrap specture console output.
/// </summary>
public class AnsiConsoleOutputWriter : IOutputWriter
{
    /// <inheritdoc/>
    public void Markup(string text, Style? style = null) => AnsiConsole.Markup(text, style.EnsureValidStyle());

    /// <inheritdoc/>
    public void MarkupLine(string text, Style? style = null) => AnsiConsole.MarkupLine(text, style.EnsureValidStyle());

    /// <inheritdoc/>
    public void Write(string text, Style? style = null) => AnsiConsole.Write(text, style.EnsureValidStyle());

    /// <summary>Writes a renderable element (table, panel, rule) to the console.</summary>
    /// <example><![CDATA[writer.Write(new Panel("Hello"));]]></example>
    public void Write(IRenderable element) => AnsiConsole.Write(element);

    /// <summary>Writes a formatted exception to the console for diagnostic purposes.</summary>
    /// <example><![CDATA[writer.WriteException(ex);]]></example>
    public void WriteException(Exception ex)
        => AnsiConsole.WriteException(ex);

    /// <summary>Writes a blank line to the console.</summary>
    /// <example><![CDATA[writer.WriteLine();]]></example>
    public void WriteLine() => AnsiConsole.WriteLine();

    public void WriteLine(string text, Style? style = null) => AnsiConsole.WriteLine(text, style.EnsureValidStyle());

    /// <inheritdoc/>
    public void WriteRule() => AnsiConsole.Write(new Rule());
}
