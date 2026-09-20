namespace Ragnar.Output;

/// <summary>Resolves target directory paths for output artefacts.</summary>
/// <example><![CDATA[var dir = resolver.ResolveResponseDirectory(cat);]]></example>
public interface IPathResolver
{
    /// <summary>Constructs the full response output directory path.</summary>
    /// <param name="category">Question category determining the subfolder.</param>
    /// <returns>The resolved absolute directory path string.</returns>
    /// <example><![CDATA[string d = resolver.ResolveResponseDirectory(XML);]]></example>
    string ResolveResponseDirectory(QuestionCategory? category);
}
