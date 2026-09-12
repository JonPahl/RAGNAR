namespace Ragnar.Output;

/// <summary>Interface for formatting output data into Markdown strings.</summary>
/// <example><![CDATA[string md = formatter.Format(details);]]></example>
public interface IOutputFormatter
{
    /// <summary>Gets or sets the file extension used by this formatter.</summary>
    /// <example><![CDATA[string ext = formatter.FileExtension; //"md"]]></example>
    string FileExtension { get; set; }

    /// <summary>Formats the provided save details into a Markdown string.</summary>
    /// <param name="details">The SaveDetails object containing metadata and content.</param>
    /// <returns>A formatted Markdown string ready for persistence.</returns>
    /// <example><![CDATA[string md = formatter.Format(details);]]></example>
    string Format(SaveDetails details);
}
