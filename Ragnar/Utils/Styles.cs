namespace Ragnar.Utils;

/// <summary>Static class providing reusable console styling options. </summary>
public static class Styles
{
    /// <summary>Gets green blinking style for console output.</summary>
    /// <value>A <see cref="Style"/> with green foreground and slow blink.</value>
    /// <example><![CDATA[Console.WriteLine("Alert!", Styles.GreenBlink);]]></example>
    public static Style GreenBlink => new()
    { Foreground = Color.Green, Decoration = Decoration.SlowBlink };

    /// <summary>Gets green foreground style for console output.</summary>
    /// <value>A <see cref="Style"/> with green foreground.</value>
    /// <example><![CDATA[Console.WriteLine("Success", Styles.Green);]]></example>
    public static Style Green => new()
    { Foreground = Color.Green };

    /// <summary>Gets yellow foreground style for console output.</summary>
    /// <value>A <see cref="Style"/> with yellow foreground.</value>
    /// <example><![CDATA[Console.WriteLine("Warning", Styles.Yellow);]]></example>
    public static Style Yellow => new()
    { Foreground = Color.Yellow };

    /// <summary>Gets cyan bold style for console output.</summary>
    /// <value>A <see cref="Style"/> with cyan foreground and bold decoration.</value>
    /// <example><![CDATA[Console.WriteLine("Info", Styles.Cyan);]]></example>
    public static Style Cyan => new()
    { Foreground = Color.Cyan, Decoration = Decoration.Bold };

    /// <summary>Gets blue foreground style for console output.</summary>
    /// <value>A <see cref="Style"/> with blue foreground.</value>
    /// <example><![CDATA[Console.WriteLine("Note", Styles.Blue);]]></example>
    public static Style Blue => new()
    { Foreground = Color.Blue };

    /// <summary>Gets bold italic blue style for console output.</summary>
    /// <value>A <see cref="Style"/> with blue foreground and bold+italic decorations.</value>
    /// <example><![CDATA[Console.WriteLine("Highlight", Styles.BoldBlue);]]></example>
    public static Style BoldBlue => new()
    { Foreground = Color.Blue, Decoration = Decoration.Bold | Decoration.Italic };

    /// <summary>Gets bold italic steel blue style for console output.</summary>
    /// <value>A <see cref="Style"/> with steel blue foreground and bold+italic decorations.</value>
    /// <example><![CDATA[Console.WriteLine("Emphasis", Styles.BoldSteelBlue);]]></example>
    public static Style BoldSteelBlue => new()
    { Foreground = Color.SteelBlue, Decoration = Decoration.Bold | Decoration.Italic };
}
