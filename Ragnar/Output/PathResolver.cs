namespace Ragnar.Output;

/// <summary>PathResolver: Resolves target directory paths based on source and question category.</summary>
/// <param name="options">Ragnar config providing source and output folder paths.</param>
/// <remarks>Implements IPathResolver for consistent output folder structure management.</remarks>
/// <example><![CDATA[var dir = resolver.ResolveResponseDirectory(cat);]]></example>
public sealed class PathResolver(IOptions<RagnarConfig> options)
    : IPathResolver
{

    private readonly ApplicationOptions _applicationOptions = options.Value.ApplicationOptions ?? throw new ArgumentNullException(nameof(options), "RagnarConfig options cannot be null.");


    /// <summary>Constructs full response path from source and category.</summary>
    /// <param name="category">Question category used to determine subfolder selection.</param>
    /// <remarks>Falls back to Uncategorized if category is null or empty string.</remarks>
    /// <example><![CDATA[var dir = resolver.ResolveResponseDirectory(src, cat);]]></example>
    /// <returns>The resolved target directory path string.</returns>
    public string ResolveResponseDirectory(
        QuestionCategory? category)
    {
        var baseDir = category is null || string.IsNullOrWhiteSpace(category.ToString()) ? nameof(QuestionCategory.Uncategorized) : category.ToString();

        var responseDir = Path.Join(_applicationOptions.SourceDirectory, _applicationOptions.OutputFolder);

        if (!Directory.Exists(responseDir))
            throw new DirectoryNotFoundException(responseDir);

        return Path.Join(responseDir, baseDir);
    }
}
