namespace Ragnar.Output;

/// <summary>
/// Interface for content formatter's, providing a standardized way to format output.
/// </summary>
public interface IContentFormatter
{
    /// <summary>Gets or sets the file extension used by this formatter.</summary>
    /// <returns>The file extension string, e.g., "md" for Markdown output.</returns>
    /// <example><![CDATA[string ext = formatter.FileExtension; //"md"]]>></example>
    string FileExtension { get; }

    /// <summary>Formats the content based on the provided details.</summary>
    /// <param name="details">The save details to format with.</param>
    /// <returns>The formatted content as a string.</returns>
    string Format(ResponseRecord details);
}
