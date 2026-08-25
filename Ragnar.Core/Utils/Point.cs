namespace Ragnar.Core.Utils;

/// <summary>
/// Provides deterministic ID generation for vector store points.
/// </summary>
public static class Point
{
    /// <summary>Creates a deterministic 64-bit point ID from filename and index.</summary>
    /// <param name="Path">File path.</param>
    /// <param name="Index">chunk int index.</param>
    /// <returns>Unique PointId.</returns>
    /// <example><![CDATA[PointId id = Utils.CreateStringPointId("file.cs", 5);]]></example>
    public static ulong FromFilePathAndIndex(
        in ReadOnlySpan<char> Path,
        in ReadOnlySpan<char> Index)
    {
        if (Index.IsEmpty || Index.IsWhiteSpace())
            throw new ArgumentException("Index cannot be null or whitespace.", nameof(Index));

        var normalizedPath =
            System.IO.Path.GetFullPath(Path.ToString())
            .AsSpan()
            .TrimEnd(System.IO.Path.DirectorySeparatorChar);
        var combined = $"{normalizedPath}_{Index}";

        return Fnv1a.Hash64(Encoding.UTF8.GetBytes(combined));
    }
}
