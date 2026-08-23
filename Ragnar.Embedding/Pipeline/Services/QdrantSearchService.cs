namespace Ragnar.Embedding.Pipeline.Services;

/// <summary>Initializes a new instance of the vector search service.</summary>
/// <param name="Client"> Qdrant client used for database queries and searches.</param>
/// <param name="Config"> Application configuration options for embedding settings.</param>
public class QdrantSearchService(IQdrantClient Client, IOptions<RagnarConfig> Config)
    : IVectorSearchService
{
    private readonly RagnarConfig _config = Config.Value;

    /// <summary>
    /// Retrieves relevant code snippets based on vector similarity.
    /// </summary>
    /// <param name="CollectionName"></param>
    /// <param name="Vector"></param>
    /// <param name="Filter"></param>
    /// <param name="Ct"></param>
    /// <returns>Returns formatted context string containing matched code blocks.</returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<string> RetrieveContextAsync(string CollectionName, ReadOnlyMemory<float> Vector, Filter? Filter, CancellationToken Ct)
    {
        if (Convert.ToUInt64(Vector.Length)
            != _config.EmbeddingOptions.Dimension)
        {
            throw new ArgumentException($"Vector dimension mismatch. Expected {_config.EmbeddingOptions.Dimension}, got {Vector.Length}");
        }

        //var Results = await Client.QueryAsync(
        //CollectionName, filter: Filter, limit: 200, cancellationToken: Ct);

        var results = await Client.SearchAsync(
            CollectionName,
            Vector,
            filter: Filter,
            limit: 200,
            cancellationToken: Ct);

        return FormatContext(results);
    }

    /// <summary>
    /// Formats search results into a readable markdown-style text block.
    /// </summary>
    /// <param name="Results"></param>
    /// <returns></returns>
    private static string FormatContext(IReadOnlyList<ScoredPoint> Results)
    {
        //var sb = new StringBuilder();
        //foreach (var point in Results.Select(P => P.Payload.ToObject()))
        //{
        //    sb.AppendLine($"File Name: {point.FileName} | Type: {point.Type} | Element: {point.ElementName}");
        //    sb.AppendLine(point.Code);
        //}
        //return sb.ToString();

        // Estimate size to minimize reallocations
        var estimatedSize = Results.Count * 256;
        var span = new char[estimatedSize];
        var written = 0;

        foreach (var point in Results.Select(p => p.Payload.ToObject()))
        {
            // 1. Slice the span using C# range syntax or .Slice()
            var remainingSpan = span.AsSpan()[written..];

            // 2. Use TryWrite for allocation-free interpolated string formatting
            if (!remainingSpan.TryWrite(
                $"File Name: {point.FileName} | Type: {point.Type} | Element: {point.ElementName}\n{point.Code}\n",
                out var charsWritten))
            {
                // Fallback or resize logic if needed
                break;
            }
            written += charsWritten;
        }

        return new string(span.AsSpan(0, written));

    }
}

public record QdrantPayload(string Code, string FileName, string ElementName, string Type);

public static class QdrantPayloadExtensions
{
    public static QdrantPayload ToObject(this MapField<string, Value> Point)
    {
        var code = Point.GetValueOrDefault("Code", string.Empty).ToString();
        var fileName = Point.GetValueOrDefault("file_name", "N/A").StringValue;
        var elementName = Point.GetValueOrDefault("element_name", "N/A").StringValue;
        var type = Point.GetValueOrDefault("type", "N/A").StringValue;

        return new QdrantPayload(code, fileName, elementName, type);
    }
}
