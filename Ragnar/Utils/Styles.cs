namespace Ragnar.Utils;

/// <summary>Static class providing reusable console styling options.
/// </summary>
public static class Styles
{
    /// <summary>Gets green blinking style.</summary>
    /// <value>A <see cref="Style"/> with green foreground and slow blink.</value>
    /// <example><![CDATA[Console.WriteLine("Alert!", Styles.GreenBlink);]]></example>
    public static Style GreenBlink => new(foreground: Color.Green, decoration: Decoration.SlowBlink);

    /// <summary>Gets green style.</summary>
    /// <value>A <see cref="Style"/> with green foreground.</value>
    public static Style Green => new(foreground: Color.Green);

    /// <summary>Gets yellow style.</summary>
    /// <value>A <see cref="Style"/> with yellow foreground.</value>
    public static Style Yellow => new() { Foreground = Color.Yellow };

    /// <summary>Gets cyan bold style.</summary>
    /// <value>A <see cref="Style"/> with cyan foreground and bold decoration.</value>
    public static Style Cyan => new(foreground: ConsoleColor.Cyan, decoration: Decoration.Bold);

    /// <summary>Gets blue style.</summary>
    /// <value>A <see cref="Style"/> with blue foreground.</value>
    public static Style Blue => new(foreground: Color.Blue);

    /// <summary>Gets bold italic blue style.</summary>
    /// <value>A <see cref="Style"/> with blue foreground and bold+italic decorations.</value>
    public static Style BoldBlue => new(foreground: Color.Blue, decoration: Decoration.Bold | Decoration.Italic);

    /// <summary>Gets bold italic steel blue style.</summary>
    /// <value>A <see cref="Style"/> with steel blue foreground and bold+italic decorations.</value>
    public static Style BoldSteelBlue => new(foreground: Color.SteelBlue, decoration: Decoration.Bold | Decoration.Italic);
}
