namespace FileQuestionProvider;

/// <summary>Defines the contract for parsing a structured data file into entities.</summary>
/// <example><![CDATA[var recs = await parser.ParseAsync("file.csv", ct);]]></example>
public interface IRecordParser<T>
{
    /// <summary>Reads and deserialises a file into a collection of records.</summary>
    /// <param name="filePath">Path to the source data file.</param>
    /// <param name="cancellationToken">Token to cancel the read operation.</param>
    /// <returns>Enumerable of deserialised <typeparamref name="T"/> objects.</returns>
    /// <example><![CDATA[var r = await parser.ParseAsync("q.csv", ct);]]></example>
    Task<IEnumerable<T>> ParseAsync(string filePath, CancellationToken cancellationToken);
}
