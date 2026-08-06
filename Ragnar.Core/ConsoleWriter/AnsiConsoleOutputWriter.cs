namespace Ragnar.Core.ConsoleWriter;

/// <summary>Wraps Spectre.Console output methods for consistent formatting.</summary>
public class AnsiConsoleOutputWriter : IOutputWriter
{

    /// <inheritdoc/>
    public void Markup(string Text, Style? Style = null)
    {
        switch(Style)
        {
            case null:
                AnsiConsole.Markup(Text);
                break;
            default:
                AnsiConsole.Markup(Text, Style ?? Spectre.Console.Style.Plain);
                break;
        }
    }

    /// <inheritdoc/>
    public void MarkupLine(string Text, Style? Style = null)
    {
        switch(Style)
        {
            case null:
                AnsiConsole.MarkupLine(Text);
                break;
            default:
                AnsiConsole.MarkupLine(Text, Style ?? Spectre.Console.Style.Plain);
                break;
        }
    }

    /// <inheritdoc/>
    public void Write(string Text, Style? Style = null)
    {
        switch(Style)
        {
            case null:
                AnsiConsole.Write(Text);
                break;
            default:
                AnsiConsole.Write(Text, Style ?? Spectre.Console.Style.Plain);
                break;
        }
    }

    /// <inheritdoc/>
    public void Write(IRenderable Text) => AnsiConsole.Write(Text);

    /// <inheritdoc/>
    public void WriteLine() => AnsiConsole.WriteLine();

    /// <inheritdoc/>
    public void WriteLine(string Text, Style? Style = null)
    {
        switch(Style)
        {
            case null:
                AnsiConsole.WriteLine(Text);
                break;
            default:
                AnsiConsole.WriteLine(Text, Style ?? Spectre.Console.Style.Plain);
                break;
        }
    }

    /// <inheritdoc/>
    public void WriteRule() => AnsiConsole.Write(new Rule());
}
