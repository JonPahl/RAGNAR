namespace Ragnar.Core.ConsoleWriter;

/// <summary>
/// output writer to wrap specture console output.
/// </summary>
public class AnsiConsoleOutputWriter
    : IOutputWriter
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

/// <summary>Extends Spectre.Console style handling with null-safe fallback.</summary>
/// <remarks>Helper extension for safe console styling and rendering operations.</remarks>
public static class StylesExtensions
{
    /// <summary>Returns provided style or plain default if null.</summary>
    /// <param name="style">Optional Spectre.Console style instance to validate.</param>
    /// <remarks>Prevents null reference exceptions during console rendering pipelines.</remarks>
    /// <example><![CDATA[var s = myStyle?.EnsureValidStyle;]]></example>
    /// <returns>A valid Style instance, guaranteed never to be null.</returns>
    extension(Style? style)
    {
        /// <summary>Ensures a valid style is returned, defaulting to plain if null.</summary>
        /// <returns>Valid style instance.</returns>
        /// <example><![CDATA[var style = myStyle?.EnsureValidStyle();]]></example>
        public Style EnsureValidStyle()
        {
            return style ?? Spectre.Console.Style.Plain;
        }
    }
}
