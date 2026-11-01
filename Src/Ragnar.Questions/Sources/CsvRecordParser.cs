namespace Ragnar.Questions.Sources;

/// <summary>Parses a CSV file into QuestionRecord entities via CsvHelper.</summary>
/// <remarks>Registers QuestionMap for column and enum conversions.</remarks>
/// <example><![CDATA[var records = await parser.ParseAsync("q.csv", ct);]]></example>
public sealed class CsvRecordParser(Serilog.ILogger logger)
    : IRecordParser<QuestionRecord>
{
    /// <summary>Reads and deserialises a CSV file into a list of QuestionRecords.</summary>
    /// <param name="filePath">Absolute or relative path to the .csv file.</param>
    /// <param name="cancellationToken">Token to cancel the read operation.</param>
    /// <remarks>Trims whitespace; uses InvariantCulture for consistent parsing.</remarks>
    /// <example><![CDATA[var rows = await parser.ParseAsync("data.csv", ct);]]></example>
    /// <returns>Enumerable of deserialised QuestionRecord objects.</returns>
    public async Task<IEnumerable<QuestionRecord>> ParseAsync(string filePath, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrEmpty(filePath);
        logger.Information("Parsing CSV file: {FilePath}", filePath);

        var sw = Stopwatch.StartNew();

        CsvReaderOptions? options = new()
        {
            MalformedData = CsvMalformedDataHandling.Recover,
            Encoding = System.Text.Encoding.UTF8,

        };

        var x = CsvReader.FromFile(filePath, options);
        var records = await x.GetRecordsAsync<QuestionRecord>(cancellationToken).ToListAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        sw.Stop();
        logger.Information("Parsed {Count} records from {FilePath} in {Elapsed}", records.Count, filePath, sw.Elapsed);
        return records;
    }
}
