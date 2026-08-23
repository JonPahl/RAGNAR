namespace Ragnar.Core.ConsoleWriter;

public interface IOutputWriter
{
    /// <summary>
    /// Writes formatted markup to console.
    /// </summary>
    /// <param name="Text">Text to write.</param>
    /// <param name="Style">Optional style.</param>
    void Markup(string Text, Style? Style = null);

    /// <summary>
    /// Writes markup line.
    /// </summary>
    void MarkupLine(string Text, Style? Style = null);

    /// <summary>
    /// Writes plain text.
    /// </summary>
    void Write(string Text, Style? Style = null);

    /// <summary>
    /// Writes renderable.
    /// </summary>
    void Write(IRenderable Text);

    /// <summary>
    /// Writes empty line.
    /// </summary>
    void WriteLine();

    /// <summary>
    /// Writes line with optional style.
    /// </summary>
    void WriteLine(string Text, Style? Style = null);

    /// <summary>
    /// Writes horizontal rule.
    /// </summary>
    void WriteRule();

    void WriteException(Exception Ex);
}
