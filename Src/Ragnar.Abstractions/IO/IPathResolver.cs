namespace Ragnar.Abstractions.IO;

/// <summary>Resolves and manages output directory paths for categorized responses.</summary>
/// <remarks>Encapsulates folder-structure logic so callers need no path math.</remarks>
/// <example><![CDATA[var dir = resolver.ResolveResponseDirectory(cat);]]></example>
public interface IPathResolver
{
    /// <summary>Resolves the target directory for a given question category.</summary>
    /// <param name="category">The question category; null yields the "Uncategorized" folder.</param>
    /// <returns>A fully-qualified directory path string.</returns>
    /// <example><![CDATA[string dir = resolver.ResolveResponseDirectory(QuestionCategory.XML);]]></example>
    string ResolveResponseDirectory(QuestionCategory? category);

    /// <summary>Creates the directory (and parents) if it does not already exist.</summary>
    /// <param name="path">Absolute or relative directory path to ensure exists.</param>
    /// <exception cref="UnauthorizedAccessException">The process lacks write permission.</exception>
    /// <example><![CDATA[resolver.EnsureDirectoryExists(@"C:\out\XML");]]></example>
    void EnsureDirectoryExists(string path);
}
