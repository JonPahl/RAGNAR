namespace FileQuestionProvider;

/// <summary>Parses a CSV file into QuestionRecord entities via CsvHelper.</summary>
/// <remarks>Registers QuestionMap for column and enum conversions.</remarks>
/// <example><![CDATA[var records = await parser.ParseAsync("q.csv", ct);]]></example>
public sealed class CsvRecordParser
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

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
        };

        using var reader = new StreamReader(filePath);

        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<QuestionMap>();

        return await csv
            .GetRecordsAsync<QuestionRecord>(cancellationToken).ToListAsync(cancellationToken: cancellationToken);
    }
}
