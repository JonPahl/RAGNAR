using Qdrant.Client.Grpc;

using Ragnar.Core.Utils;

namespace Ragnar.Core.Model;

/// <summary>Represents a code document with source, metadata, and comments.</summary>
public sealed record CodeDocument
{
    /// <summary>Gets the filename of the code document.</summary>
    public required string FileName { get; init; }

    /// <summary>Gets the XML element type (e.g., class, method).</summary>
    public required string ElementType { get; init; }

    /// <summary>Gets the name of the XML element.</summary>
    public required string ElementName { get; init; }

    /// <summary>Gets the source code content.</summary>
    public required string Code { get; init; }

    /// <summary>Gets the associated comment string.</summary>
    public required string Comment { get; init; }
    public string? Category { get; init; }
    public required int Comment_Length { get; init; }

    public PointId AsPoint()
    {
        return string.IsNullOrWhiteSpace(ElementName)
            ? (PointId)Point.FromFilePathAndIndex(FileName, "None")
            : (PointId)Point.FromFilePathAndIndex(FileName, ElementName);
    }
}
