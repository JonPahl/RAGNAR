namespace Ragnar.Core.ConsoleWriter;

/// <summary>
/// output writer to wrap specture console output.
/// </summary>
public class AnsiConsoleOutputWriter
    : IOutputWriter
{

    /// <inheritdoc/>
    public void Markup(string Text, Style? Style = null) => AnsiConsole.Markup(Spectre.Console.Markup.Escape(Text), Style.GetStyle);

    /// <inheritdoc/>
    public void MarkupLine(string Text, Style? Style = null) => AnsiConsole.MarkupLine(Text, Style.GetStyle);

    /// <inheritdoc/>
    public void Write(string Text, Style? Style = null) => AnsiConsole.Write(Text, Style.GetStyle);

    /// <inheritdoc/>
    public void Write(IRenderable Text) => AnsiConsole.Write(Text);

    public void WriteException(Exception Ex)
        => AnsiConsole.WriteException(Ex);

    /// <inheritdoc/>
    public void WriteLine() => AnsiConsole.WriteLine();

    /// <inheritdoc/>
    public void WriteLine(string Text, Style? Style = null) => AnsiConsole.WriteLine(Text, Style.GetStyle);

    /// <inheritdoc/>
    public void WriteRule() => AnsiConsole.Write(new Rule());
}

/// <summary>Extends Spectre.Console style handling with null-safe fallback.</summary>
public static class StylesExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Style">Optional Spectre.Console style.</param>
    extension(Style? Style)
    {
        /// <summary>Ensures a valid style is returned, defaulting to plain if null.</summary>
        /// <returns>Valid style instance.</returns>
        /// <example><![CDATA[var style = myStyle?.GetStyle();]]></example>
        public Style GetStyle => Style ?? Spectre.Console.Style.Plain;
    }
}
