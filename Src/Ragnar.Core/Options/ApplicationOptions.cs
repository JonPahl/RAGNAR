namespace Ragnar.Core.Options;

/// <summary>
/// Root application options for source, vector store, and output paths.
/// </summary>
/// <example><![CDATA[opts.SourceDirectory = @"C:\src\Ragnar";]]></example>
public record ApplicationOptions
{
    /// <summary>Gets or sets whether to prepend the original prompt in output.</summary>
    /// <returns>True to include the prompt; false to omit.</returns>
    /// <example><![CDATA[opts.IncludeOriginalPrompt = true;]]></example>
    public bool IncludeOriginalPrompt { get; init; }

    public uint PageSize { get; init; } = 10;

    /// <summary>Gets or sets the directory containing source files to process.</summary>
    /// <returns>The expanded absolute path to the source directory.</returns>
    /// <example><![CDATA[opts.SourceDirectory = @"C:\src\Ragnar";]]></example>
    [Required]
    public required string SourceDirectory { get; set; } = default!;

    /// <summary>Gets the name of the vector collection for storing embeddings.</summary>
    /// <returns>The Qdrant collection name for vector storage.</returns>
    /// <example><![CDATA[opts.VectorStoreName = "ragnar_vectors";]]></example>
    [Required]
    public required string VectorStoreName { get; init; } = default!;

    /// <summary>Optional array of categories to filter; null means all.</summary>
    /// <returns>The category names, or null to process every category.</returns>
    /// <example><![CDATA[opts.CategoriesToProcess = [QuestionCategory.Performance];]]></example>
    public QuestionCategory[]? CategoriesToProcess { get; init; } = [];

    /// <summary>Gets or sets the output folder name for generated reports.</summary>
    /// <returns>The folder name; defaults to "Response."</returns>
    /// <example><![CDATA[opts.OutputFolder = "Reports";]]></example>
    public string OutputFolder { get; init; } = "Response";
}
