using HashDepot;

using System.Text;

namespace Ragnar.Core.Utils;
/// <summary>
/// Provides deterministic ID generation for vector store points.
/// </summary>
public static class Point
{
    /// <summary>Creates a deterministic 64-bit point ID from filename and index.</summary>
    /// <param name="path">File path.</param>
    /// <param name="index">chunk int index.</param>
    /// <returns>Unique PointId.</returns>
    /// <example><![CDATA[PointId id = Utils.CreateStringPointId("file.cs", 5);]]></example>
    public static ulong FromFilePathAndIndex(in ReadOnlySpan<char> path, in ReadOnlySpan<char> index)
    {
        if (index.IsEmpty || index.IsWhiteSpace())
            throw new ArgumentException("Index cannot be null or whitespace.", nameof(index));

        var normalizedPath = Path.GetFullPath(path.ToString()).AsSpan().TrimEnd(Path.DirectorySeparatorChar);
        var combined = $"{normalizedPath}_{index}";
        return Fnv1a.Hash64(Encoding.UTF8.GetBytes(combined));
    }
}
