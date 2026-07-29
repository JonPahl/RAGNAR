namespace Ragnar.Core.Utils;
/// <summary>
/// Provides deterministic ID generation for vector store points.
/// </summary>
public static class Point
{
    /// <summary>Creates a deterministic 64-bit point ID from filename and index.</summary>
    /// <param name="filePath">File filePath.</param>
    /// <param name="index">chunk int index.</param>
    /// <returns>Unique PointId.</returns>
    /// <example><![CDATA[PointId id = Utils.CreateStringPointId("file.cs", 5);]]></example>
    public static ulong FromFilePathAndIndex (in ReadOnlySpan<char> filePath, in ReadOnlySpan<char> index)
    {
        Guard.Against.NullOrWhiteSpace(filePath.ToString());
        Guard.Against.NullOrWhiteSpace(index.ToString());

        var normalizedPath = Path.GetFullPath(filePath.ToString()).AsSpan().TrimEnd(Path.DirectorySeparatorChar);

        var combined = $"{normalizedPath}_{index}";
        return Fnv1a.Hash64(Encoding.UTF8.GetBytes(combined));
    }
}
