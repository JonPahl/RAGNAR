using Spectre.Console;
using Spectre.Console.Rendering;

namespace Ragnar.Core.ConsoleWriter;
/// <summary>
/// output writer to wrap specture console output.
/// </summary>
public class AnsiConsoleOutputWriter : IOutputWriter
{

    /// <inheritdoc/>
    public void Markup(string text, Style? style = null)
    {
        switch (style)
        {
            case null:
                AnsiConsole.Markup(text);
                break;
            default:
                AnsiConsole.Markup(text, style ?? Style.Plain);
                break;
        }
    }

    /// <inheritdoc/>
    public void MarkupLine(string text, Style? style = null)
    {
        switch (style)
        {
            case null:
                AnsiConsole.MarkupLine(text);
                break;
            default:
                AnsiConsole.MarkupLine(text, style ?? Style.Plain);
                break;
        }
    }

    /// <inheritdoc/>
    public void Write(string text, Style? style = null)
    {
        switch (style)
        {
            case null:
                AnsiConsole.Write(text);
                break;
            default:
                AnsiConsole.Write(text, style ?? Style.Plain);
                break;
        }
    }

    /// <inheritdoc/>
    public void Write(IRenderable text) => AnsiConsole.Write(text);

    /// <inheritdoc/>
    public void WriteLine() => AnsiConsole.WriteLine();

    /// <inheritdoc/>
    public void WriteLine(string text, Style? style = null)
    {
        switch (style)
        {
            case null:
                AnsiConsole.WriteLine(text);
                break;
            default:
                AnsiConsole.WriteLine(text, style ?? Style.Plain);
                break;
        }
    }

    /// <inheritdoc/>
    public void WriteRule() => AnsiConsole.Write(new Rule());
}
