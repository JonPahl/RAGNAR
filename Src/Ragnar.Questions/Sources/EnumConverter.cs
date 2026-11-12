namespace Ragnar.Questions.Sources;

/// <summary>Converts CSV cell values to and from the QuestionCategory enum.</summary>
/// <example><![CDATA[var c = new EnumConverter();]]></example>
public class EnumConverter : ICsvConverter<QuestionCategory>
{
    /// <summary>Reads a CSV cell and parses it into a QuestionCategory value.</summary>
    /// <param name="reader">The CSV reader positioned at the target row.</param>
    /// <param name="index">Zero-based column index to read.</param>
    /// <returns>The parsed QuestionCategory, or UnCategorized on parse failure.</returns>
    /// <example><![CDATA[QuestionCategory cat = conv.Read(reader, 0);]]></example>
    public QuestionCategory Read(CsvReader reader, int index)
    {
        var value = reader.GetString(index);

        return Enum.TryParse<QuestionCategory>(value, out var match) ? match : QuestionCategory.Uncategorized;
    }

    /// <summary>Writes a QuestionCategory value to a CSV cell.</summary>
    /// <param name="writer">The CSV writer positioned at the target row.</param>
    /// <param name="value">The QuestionCategory value to write.</param>
    /// <example><![CDATA[conv.Write(writer, QuestionCategory.Support);]]></example>
    public void Write(CsvWriter writer, QuestionCategory value) => writer.WriteField(value);
}
