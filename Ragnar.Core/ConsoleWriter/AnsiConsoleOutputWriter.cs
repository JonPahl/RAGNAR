namespace Ragnar.Core.ConsoleWriter;

/// <summary>
/// output writer to wrap specture console output.
/// </summary>
public class AnsiConsoleOutputWriter : IOutputWriter
{

    /// <inheritdoc/>
    public void Markup(string text, Style? style = null) => AnsiConsole.Markup(text, style);

    /// <inheritdoc/>
    public void MarkupLine(string text, Style? style = null) => AnsiConsole.MarkupLine(text, style);

    /// <inheritdoc/>
    public void Write(string text, Style? style = null)
    {
        switch(style)
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
        switch(style)
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
