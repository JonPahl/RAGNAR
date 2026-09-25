namespace Ragnar.Output;

/// <summary>Interface for formatting output data into Markdown strings.</summary>
/// <example><![CDATA[string md = formatter.FormatResponse(details);]]></example>
public interface IOutputFormatter
{
    /// <summary>Gets or sets the file extension used by this formatter.</summary>
    /// <returns>The output file extension string (e.g., "md").</returns>
    /// <example><![CDATA[string ext = formatter.FileExtension;]]></example>
    string FileExtension { get; set; }

    /// <summary>Formats the provided save details into a Markdown string.</summary>
    /// <param name="details">The ResponseRecord object containing metadata and content.</param>
    /// <returns>A formatted Markdown string ready for persistence.</returns>
    /// <example><![CDATA[string md = formatter.FormatResponse(details);]]></example>
    string FormatResponse(ResponseRecord details);
}
