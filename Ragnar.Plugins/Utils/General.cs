namespace Ragnar.Plugins.Utils;

/// <summary>
/// A utility class providing general-purpose functionality.
/// </summary>
public static class General
{
    /// <summary>
    /// The culture used for formatting dates, numbers, and other string representations.
    /// </summary>
    public static System.Globalization.CultureInfo Culture { get; } = new("en-US");

    /// <summary>
    /// A culture that is invariant across different systems and locales.
    /// </summary>
    public static System.Globalization.CultureInfo InvariantCulture { get; } = System.Globalization.CultureInfo.InvariantCulture;
}
