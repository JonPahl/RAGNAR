namespace FileQuestionProvider;

/// <summary>QuestionMap.cs Configures CSV mapping for QuestionRecord fields.</summary>
/// <example><![CDATA[var map = new QuestionMap();]]></example>
public sealed class QuestionMap
    : ClassMap<QuestionRecord>
{
    public QuestionMap()
    {
        Map(M => M.IsEnabled)
            .Name("IsEnabled")
            .TypeConverterOption.BooleanValues(true, true, "1")
            .TypeConverterOption.BooleanValues(false, true, "0");

        Map(M => M.Text).Name("Text");
        Map(M => M.FileName).Name("FileName");
        Map(M => M.Category).Name("Category")
            .Convert(Args =>
            {
                var Value = Args.Row.GetField<string>("Category");

                if(Enum.TryParse<QuestionCategory>(Value, true, out var Result))
                {
                    return Result;
                }

                return QuestionCategory.General;
            });
    }
}
