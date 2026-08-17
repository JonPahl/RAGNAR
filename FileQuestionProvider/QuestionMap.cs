namespace FileQuestionProvider;

/// <summary>Maps CSV columns to question configuration properties using CsvHelper conventions.</summary>
public sealed class QuestionMap
    : ClassMap<QuestionRecord>
{
    /// <summary>Configures column mappings, boolean parsing, and enum conversion for categories.</summary>
    public QuestionMap()
    {
        Map(m => m.IsEnabled).Name("IsEnabled")
            .TypeConverterOption.BooleanValues(true, true, "1")
            .TypeConverterOption.BooleanValues(false, true, "0");

        Map(m => m.Text).Name("Text");
        Map(m => m.FileName).Name("FileName");
        Map(m => m.Category).Name("Category")
            .Convert(args =>
            {
                var value = args.Row.GetField<string>("Category");

                if(Enum.TryParse<QuestionCategory>(value, true, out var result))
                {
                    return result;
                }

                return QuestionCategory.General;
            });
    }
}
