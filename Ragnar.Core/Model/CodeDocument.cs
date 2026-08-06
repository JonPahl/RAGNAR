namespace Ragnar.Core.Model;

/// <summary>Represents a code document with source, metadata, and comments.</summary>
public sealed record CodeDocument(
    string FileName,
    string ElementType,
    string ElementName,
    string Comment,
    int CommentLength,
    string Code,
    string Category = "General")
{
    public PointId AsPoint()
    {
        return string.IsNullOrWhiteSpace(ElementName)
            ? Point.FromFilePathAndIndex(FileName, "None")
            : Point.FromFilePathAndIndex(FileName, ElementName);
    }
}
