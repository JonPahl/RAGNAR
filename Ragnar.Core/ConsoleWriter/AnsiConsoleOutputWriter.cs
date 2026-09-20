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


    public void Write(IRenderable text) => AnsiConsole.Write(text);

    public void WriteException(Exception ex)
        => AnsiConsole.WriteException(ex);

    public void WriteLine() => AnsiConsole.WriteLine();


    public void WriteLine(string text, Style? style = null) => AnsiConsole.WriteLine(text, style.EnsureValidStyle());

    /// <inheritdoc/>
    public void WriteRule() => AnsiConsole.Write(new Rule());
}
