namespace Ragnar.Utils;

/// <summary>
/// Custom console style classes.
/// </summary>
public static class Styles
{
    /// <summary>
    /// Gets green blinking style.
    /// </summary>
    public static Style GreenBlink => new(foreground: Color.Green, decoration: Decoration.SlowBlink);

    /// <summary>
    /// Gets yellow style.
    /// </summary>
    public static Style Yellow => new() { Foreground = Color.Yellow };

    /// <summary>
    /// Gets cyan style.
    /// </summary>
    public static Style Cyan => new(foreground: ConsoleColor.Cyan);

    /// <summary>
    /// Gets blue style.
    /// </summary>
    public static Style Blue => new(foreground: Color.Blue);

    /// <summary>
    /// Gets bold blue style.
    /// </summary>
    public static Style BoldBlue => new(foreground: Color.Blue, decoration: Decoration.Bold | Decoration.Italic);

    /// <summary>
    /// Gets Bold Steel Blue style.
    /// </summary>
    public static Style BoldSteelBlue => new(foreground: Color.SteelBlue, decoration: Decoration.Bold | Decoration.Italic);
}
