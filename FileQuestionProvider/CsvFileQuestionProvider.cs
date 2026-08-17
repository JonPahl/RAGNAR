namespace FileQuestionProvider;

/// <summary>Loads questions from CSV files into configuration objects for processing.</summary>
public class CsvFileQuestionProvider : IQuestionProvider
{
    public string ProviderName => "CSV File";

    private string FileName = string.Empty;


    /// <summary>Sets the target CSV file path for subsequent loading operations.</summary>
    /// <param name="fileName">Absolute or relative path to the CSV file.</param>
    public void SetFileName(string fileName)
    {
        FileName = fileName;
    }

    /// <summary>Initiates asynchronous loading of questions from the configured CSV source.</summary>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns>A task representing the asynchronous load operation with configured questions.</returns>
    /// <example><![CDATA[var q = await provider.LoadQuestionAsync(ct);]]></example>
    public async Task<IEnumerable<QuestionConfiguration>> LoadQuestionAsync(CancellationToken ct) => await ReadCsvFile();


    /// /// <summary>Parses the CSV file and maps records into QuestionConfiguration objects.</summary>
    /// <returns>A task containing an enumerable of loaded question configurations.</returns>
    private async Task<IEnumerable<QuestionConfiguration>> ReadCsvFile()
    {
        var questions = new List<QuestionConfiguration>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
        };

        using(var Reader = new StreamReader(FileName))
        using(var Csv = new CsvReader(Reader, config))
        {
            Csv.Context.RegisterClassMap<QuestionMap>();

            foreach(var record in Csv.GetRecords<QuestionRecord>())
            {
                var question = new QuestionConfiguration
                (
                    IsActive: record.IsEnabled,
                    Text: record.Text,
                    FileName: record.FileName,
                    Category: record.Category
                );
                questions.Add(question);
            }
        }

        return questions;
    }
}
