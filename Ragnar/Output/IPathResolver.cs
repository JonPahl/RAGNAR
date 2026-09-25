namespace Ragnar.Output;

/// <summary>Resolves target directory paths based on source and question category.</summary>
/// <example><![CDATA[var dir = resolver.ResolveResponseDirectory(cat);]]></example>
public interface IPathResolver
{
    /// <summary>Constructs and returns the full response output directory path.</summary>
    /// <param name="category">Question category determining the subfolder name.</param>
    /// <returns>The resolved absolute target directory path string.</returns>
    /// <example><![CDATA[string d = resolver.ResolveResponseDirectory(cat);]]></example>
    string ResolveResponseDirectory(QuestionCategory? category);

    /// <summary>Ensures the resolved directory exists (idempotent).</summary>
    void EnsureDirectoryExists(string path);
}
