
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Ragnar.Core.ConsoleWriter;

public interface IOutputWriter
{
    /// <summary>
    /// Writes formatted markup to console.
    /// </summary>
    /// <param name="text">Text to write.</param>
    /// <param name="style">Optional style.</param>
    void Markup(string text, Style? style = null);

    /// <summary>
    /// Writes markup line.
    /// </summary>
    void MarkupLine(string text, Style? style = null);

    /// <summary>
    /// Writes plain text.
    /// </summary>
    void Write(string text, Style? style = null);

    /// <summary>
    /// Writes renderable.
    /// </summary>
    void Write(IRenderable text);

    /// <summary>
    /// Writes empty line.
    /// </summary>
    void WriteLine();

    /// <summary>
    /// Writes line with optional style.
    /// </summary>
    void WriteLine(string text, Style? style = null);

    /// <summary>
    /// Writes horizontal rule.
    /// </summary>
    void WriteRule();
}
