namespace Ragnar.Extensions;

/// <summary>
/// extensions to handle if file path should be used or not.
/// </summary>
public static class PathExtensions
{
    /// <summary>Determines if filename matches any exclusion pattern (case-insensitive).</summary>
    /// <param name="FileName">The filename to check.</param>
    /// <param name="Exclusions">List of exclusion patterns.</param>
    /// <returns>True if excluded; otherwise false.</returns>
    /// <example><![CDATA[var isExcluded = "file.cs".AsSpan().IsExcluded(new[] { "FILE.CS" });]]></example>
    public static bool IsExcluded(
        this ReadOnlySpan<char> FileName,
        in ImmutableHashSet<string>? Exclusions)
    {
        if(FileName.IsEmpty || Exclusions is null or { Count: 0 })
            return false;

        foreach(var Exclusion in Exclusions)
        {
            if(FileName.Equals(Exclusion.AsSpan(), StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
