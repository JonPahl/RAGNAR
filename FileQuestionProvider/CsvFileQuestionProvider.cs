using CsvHelper;
using CsvHelper.Configuration;

using Ragnar.Plugins;
using Ragnar.Questions;

using System.Globalization;

namespace FileQuestionProvider;

public class CsvFileQuestionProvider : IQuestionProvider
{
    public string ProviderName => "CSV File";
    private string _fileName = string.Empty;

    public void SetFileName(string fileName)
    {
        _fileName = fileName;
    }

    /// <summary>Loads all active and config questions for processing.</summary>
    /// <param name="ct">Cancellation Token.</param>
    public async Task<IEnumerable<QuestionConfiguration>> LoadQuestionAsync(CancellationToken ct) => await ReadCsvFile();


    /// <summary>Loads questions from CSV file asynchronously.</summary>
    private async Task<IEnumerable<QuestionConfiguration>> ReadCsvFile()
    {
        var questions = new List<QuestionConfiguration>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
        };

        using (var reader = new StreamReader(_fileName))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<QuestionMap>();

            foreach (var record in csv.GetRecords<QuestionRecord>())
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