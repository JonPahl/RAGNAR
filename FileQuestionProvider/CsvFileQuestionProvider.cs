namespace FileQuestionProvider;

/// <summary>Initializes with CSV parser for question loading.</summary>
/// <param name="csvParser">CSV record parser used to deserialize file data.</param>
/// <remarks>Implements IQuestionProvider to handle CSV-based data ingestion.</remarks>
/// <example><![CDATA[var provider = new CsvFileQuestionProvider(parser);]]></example>
public sealed class CsvFileQuestionProvider(IRecordParser<QuestionRecord> csvParser) : IQuestionProvider
{

    /// <summary>Gets the display name identifying this question provider.</summary>
    /// <example><![CDATA[string n = provider.ProviderName;]]></example>
    public string ProviderName => "CSV File";

    /// <summary>Parses CSV and maps to Question objects.</summary>
    /// <param name="fileName">Path to the CSV file containing question records.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous parsing operation.</param>
    /// <remarks>Uses CsvParser.ParseAsync for efficient stream processing.</remarks>
    /// <example><![CDATA[var questions = await provider.LoadQuestionsAsync("data.csv", ct);]]></example>
    /// <returns>Collection of loaded Question objects from the CSV file.</returns>
    public async Task<IEnumerable<Question>> LoadQuestionsAsync(string fileName, CancellationToken cancellationToken)
    {
        var records = await csvParser.ParseAsync(fileName, cancellationToken).ConfigureAwait(false);

        return records.Select(r => new Question(
            IsActive: r.IsEnabled, Text: r.Text, FileName: r.FileName, Category: r.Category));
    }
}
