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
    void Markup(string text, Style? style = null);

    /// <summary>
    /// Writes markup line.
    /// </summary>
    void MarkupLine(string text, Style? style = null);

    /// <summary>
    /// Writes plain element.
    /// </summary>
    void Write(string text, Style? style = null);

    /// <summary>Writes a styled console element without appending a newline.</summary>
    /// <param name="element">The Spectre.Console element to render.</param>
    /// <example><![CDATA[writer.Write(new Text("Hello", Styles.Bold));]]></example>
    void Write(IRenderable element);

    /// <summary>Writes a newline to the console output stream.</summary>
    /// <example><![CDATA[writer.WriteLine();]]></example>
    void WriteLine();

    /// <summary>
    /// Writes line with optional style.
    /// </summary>
    void WriteLine(string text, Style? style = null);

    /// <summary>Writes a horizontal rule separator to the console.</summary>
    /// <example><![CDATA[writer.WriteRule();]]></example>
    void WriteRule();

    void WriteException(Exception ex);
}
