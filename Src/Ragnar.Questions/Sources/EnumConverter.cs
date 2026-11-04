namespace Ragnar.Questions.Sources;

public class EnumConverter : ICsvConverter<QuestionCategory>
{
    public QuestionCategory Read(CsvReader reader, int index)
    {
        var value = reader.GetString(index);

        return Enum.TryParse<QuestionCategory>(value, out var match) ? match : QuestionCategory.Uncategorized;
    }

    public void Write(CsvWriter writer, QuestionCategory value) => writer.WriteField(value);
}
