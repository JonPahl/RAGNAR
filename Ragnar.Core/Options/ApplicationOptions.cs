
using System.ComponentModel.DataAnnotations;

namespace Ragnar.Core.Options;
/// <summary>
/// Application configuration options for processing source files and vector data.
/// </summary>
public record ApplicationOptions
{
    public bool IncludeOriginalPrompt { get; init; } = false;

    /// <summary>
    /// Gets or sets the directory containing source files to process.
    /// </summary>
    [Required]
    public required string SourceDirectory { get; set; } = default!;

    /// <summary>
    /// Gets the name of the vector collection for storing embeddings.
    /// </summary>
    [Required]
    public required string VectorStoreName { get; init; } = default!;

    /// <summary>
    /// Gets optional array of category names to process; defaults to all.
    /// <![CDATA[
    /// var options = new ApplicationOptions
    /// {
    ///     SourceDirectory = "/data/sources",
    ///     VectorStoreName = "my_vectors"
    /// };
    /// ]]>
    /// </summary>
    public string[]? CategoriesToProcess { get; init; } = [];
}
