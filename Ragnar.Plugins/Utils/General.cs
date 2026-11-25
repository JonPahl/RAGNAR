namespace Ragnar.Plugins.Utils;

/// <summary>Contains common utility methods for path and string handling.</summary>
public static class General
{
    /// <summary>Culture used for formatting dates, numbers, and strings.</summary>
    /// <returns>A valid culture info instance for consistent localization.</returns>
    /// <example><![CDATA[var c = General.Culture;]]></example>
    public static System.Globalization.CultureInfo Culture { get; } = new("en-US");

    /// <summary>
    /// A culture that is invariant across different systems and locales.
    /// </summary>
    public static System.Globalization.CultureInfo InvariantCulture { get; } = System.Globalization.CultureInfo.InvariantCulture;
}
