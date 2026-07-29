namespace Ragnar.Extensions;

/// <summary>
/// extensions to handle if file path should be used or not.
/// </summary>
public static class PathExtensions
{
    /// <summary>PathExtensions.cs IsExcluded checks filename against exclusion list.</summary>
    /// <param name="fileName">Filename to verify.</param>
    /// <param name="exclusions">Collection of excluded names.</param>
    /// <returns>True if excluded; otherwise false.</returns>
    /// <example><![CDATA[bool res = fileName.IsExcluded(excl);]]></example>
    public static bool IsExcluded(this in ReadOnlySpan<char> fileName, in IReadOnlyCollection<string> exclusions)
        => exclusions.Contains(fileName.ToString(), StringComparer.OrdinalIgnoreCase);
}
