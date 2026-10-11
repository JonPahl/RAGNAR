namespace Ragnar.Core.Models;

/// <summary>Represents a code document with source, metadata, and comments.</summary>
public sealed record CodeDocument
{
    /// <summary>Gets the filename of the code document.</summary>
    /// <returns>The source filename, e.g. "Program.cs".</returns>
    /// <example><![CDATA[string f = doc.FileName;]]></example>
    public required string FileName { get; set; }

    public required string FilePath { get; set; }

    /// <summary>Gets the XML element type (e.g., class, method).</summary>
    /// <returns>The Roslyn element kind, e.g. "Class" or "Method".</returns>
    /// <example><![CDATA[string t = doc.ElementType;]]></example>
    public required string ElementType { get; set; }

    /// <summary>Gets the name of the XML element.</summary>
    /// <returns>The identifier name of the declared element.</returns>
    /// <example><![CDATA[string n = doc.ElementName;]]></example>
    public required string ElementName { get; set; }

    /// <summary>Gets the source code content.</summary>
    /// <returns>The raw source-code text of the element.</returns>
    /// <example><![CDATA[string c = doc.Code;]]></example>
    public required string Code { get; set; }

    /// <summary>Gets the associated comment string.
    /// </summary>
    /// <returns>The XML documentation comment associated with the element.</returns>
    /// <example><![[CDATA[[string cm = doc.Comment;]]]]>
    /// </example>
    public required string Comment { get; set; }

    /// <summary>Gets the optional category tag for this document.</summary>
    /// <returns>Optional category tag, or null when unclassified.</returns>
    /// <example><![CDATA[string? cat = doc.Category;]]>
    /// </example>
    public string? Category { get; set; }

    /// <summary>Gets the character length of the associated comment.</summary>
    /// <returns>Character count of the associated comment text.</returns>
    /// <example><![CDATA[int len = doc.CommentLength;]]></example>
    public required int CommentLength { get; set; }

    /// <summary>Derives a deterministic PointId from the filename and element name.</summary>
    /// <returns>The computed <see cref="PointId"/> for the vector store.</returns>
    /// <example><![CDATA[PointId id = doc.AsPoint();]]></example>
    public PointId AsPoint()
    {
        return string.IsNullOrWhiteSpace(ElementName)
            ? (PointId)Point.FromFilePathAndIndex(FileName, "None")
            : (PointId)Point.FromFilePathAndIndex(FileName, ElementName);
    }
}
