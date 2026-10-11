namespace Ragnar.Core.ConsoleWriter;

/// <summary>Extends Spectre.Console style handling with null-safe fallback.</summary>
/// <remarks>Helper extension for safe console styling and rendering operations.</remarks>
/// <example><![CDATA[var s = myStyle.EnsureValidStyle();]]></example>
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
        public Style EnsureValidStyle() => style ?? Style.Plain;
    }
}
