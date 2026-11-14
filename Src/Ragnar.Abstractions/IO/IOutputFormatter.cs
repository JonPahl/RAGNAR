namespace Ragnar.Abstractions.IO;

/// <summary>Formats application output into a target document representation.</summary>
/// <remarks>Decouples content rendering from file I/O for testability.</remarks>
/// <example><![CDATA[string md = formatter.FormatResponse(record);]]></example>
public interface IOutputFormatter
{
    /// <summary>Gets the file extension (without dot) used for output files.</summary>
    /// <returns>A lowercase extension string, e.g. "md" or "html".</returns>
    /// <example><![CDATA[string ext = formatter.FileExtension;]]></example>
    string FileExtension { get; }

    /// <summary>Renders a complete output document from the supplied response record.</summary>
    /// <param name="record">The response record containing question, answer, and timing data.</param>
    /// <returns>A fully formatted document string ready for persistence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="record"/> is null.</exception>
    /// <example><![CDATA[string md = formatter.FormatResponse(record);]]></example>
    string FormatResponse(ResponseRecord details);
}
