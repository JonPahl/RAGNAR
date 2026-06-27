namespace Ragnar.Extensions;

/// <summary>
/// extensions to handle if file path should be used or not.
/// </summary>
public static class PathExtensions
{
    /// <summary>Checks if a filename is in the exclusion list (case-insensitive).</summary>
    /// <param name="fileName">Filename to check.</param>
    /// <param name="exclusions">Set of excluded filenames.</param>
    /// <returns><c>true</c> if excluded; otherwise <c>false</c>.</returns>
    public static bool IsExcluded(this in ReadOnlySpan<char> fileName, in IReadOnlyCollection<string> exclusions)
        => exclusions.Contains(fileName.ToString(), StringComparer.OrdinalIgnoreCase);
}
