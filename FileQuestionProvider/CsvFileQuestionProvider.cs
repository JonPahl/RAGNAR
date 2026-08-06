namespace FileQuestionProvider;



public class CsvFileQuestionProvider
    : IQuestionProvider
{
    public string ProviderName => "CSV File";
    private string FileName = string.Empty;


    public void SetFileName(string FileName)
    {
        this.FileName = FileName;
    }

    /// <summary>Loads all active and config questions for processing.</summary>
    /// <param name="Ct">Cancellation Token.</param>
    public async Task<IEnumerable<QuestionConfiguration>> LoadQuestionAsync(CancellationToken Ct) => await ReadCsvFile();


    /// <summary>Loads questions from CSV file asynchronously.</summary>
    private async Task<IEnumerable<QuestionConfiguration>> ReadCsvFile()
    {
        var Questions = new List<QuestionConfiguration>();

        var Config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
        };

        using(var Reader = new StreamReader(FileName))
        using(var Csv = new CsvReader(Reader, Config))
        {
            Csv.Context.RegisterClassMap<QuestionMap>();

            foreach(var Record in Csv.GetRecords<QuestionRecord>())
            {
                var Question = new QuestionConfiguration
                (
                    IsActive: Record.IsEnabled,
                    Text: Record.Text,
                    FileName: Record.FileName,
                    Category: Record.Category
                );
                Questions.Add(Question);
            }
        }

        return Questions;
    }
}
