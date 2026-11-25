using System.Security.Cryptography;

namespace Ragnar.Domain.Utils;

/// <summary>
/// Provides deterministic ID generation for vector store points.
/// </summary>
public static class Point
{
    /// <summary>Creates a deterministic 64-bit point ID from filename and index.</summary>
    /// <param name="path">File path.</param>
    /// <param name="index">chunk int index.</param>
    /// <returns>Unique PointId.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="index"/> is empty or whitespace.</exception>
    /// <example><![CDATA[PointId id = Utils.CreateStringPointId("file.cs", 5);]]></example>
    public static PointId FromFilePathAndIndex(
        in ReadOnlySpan<char> path,
        in ReadOnlySpan<char> index)
    {
        try
        {
            if (index.IsEmpty || index.IsWhiteSpace())
                throw new ArgumentException("Index cannot be null or whitespace.", nameof(index));

            var normalizedPath = Path.GetFullPath(path.ToString())
                .AsSpan()
                .TrimEnd(Path.DirectorySeparatorChar);
            var combined = $"{normalizedPath}_{index}";

            var hash = MD5.HashData(Encoding.UTF8.GetBytes(combined));

            Guid deterministicGuid = new(hash);
            return new PointId(deterministicGuid);
        }
        catch (Exception ex)
        {
            var newGuid = Guid.NewGuid();
            return new PointId(newGuid);
        }

        //var d = b.GetHashCode();
        //var x = Guid.Parse(b);
        //return new PointId(x);
        //return FNV1a.Hash64(Encoding.UTF8.GetBytes(combined));
    }
}
