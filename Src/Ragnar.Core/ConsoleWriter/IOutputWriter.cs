namespace Ragnar.Core.ConsoleWriter;

/// <summary>Abstraction for writing styled element elements to the console.</summary>
/// <example><![CDATA[writer.Write(element); writer.WriteRule();]]></example>
public interface IOutputWriter
{
    /// <summary>
    /// Writes formatted markup to console.
    /// </summary>
    /// <param name="text">element to write.</param>
    /// <param name="style">Optional style.</param>
    /// <example><![CDATA[writer.Markup("[b]Bold[/]");]></example>
    void Markup(string text, Style? style = null);

    /// <summary> Writes markup line. </summary>
    /// <param name="text">Markup string to render.</param>
    /// <param name="style">Optional Spectre.Console style.</param>
    /// <example><![CDATA[writer.MarkupLine("[green]OK[/]");]></example>
    void MarkupLine(string text, Style? style = null);

    /// <summary>Writes plain element.</summary>
    /// <param name="text">Plain text to write.</param>
    /// <param name="style">Optional Spectre.Console style.</param>
    /// <example><![CDATA[writer.Write("Hello");]></example>
    void Write(string text, Style? style = null);

    /// <summary>Writes a styled console element without appending a newline.</summary>
    /// <param name="element">The Spectre.Console element to render.</param>
    /// <example><![CDATA[writer.Write(new Text("Hello", Styles.Bold));]]></example>
    void Write(IRenderable element);

    /// <summary>Writes a newline to the console output stream.</summary>
    /// <example><![CDATA[writer.WriteLine();]]></example>
    void WriteLine();

    /// <summary>Writes line with optional style.</summary>
    /// <param name="text">Line text to write.</param>
    /// <param name="style">Optional Spectre.Console style.</param>
    /// <example><![CDATA[writer.WriteLine("Done.");]></example>
    void WriteLine(string text, Style? style = null);

    /// <summary>Writes a horizontal rule separator to the console.</summary>
    /// <example><![CDATA[writer.WriteRule();]]></example>
    void WriteRule();

    /// <summary>Writes a formatted exception message to the console.</summary>
    /// <param name="ex">The exception to display.</param>
    /// <example><![CDATA[writer.WriteException(ex);]></example>
    void WriteException(Exception ex);
}
