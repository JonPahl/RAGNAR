namespace Ragnar.Questions.Sources;

/// <summary>
/// Gets or sets the source of questions.
/// </summary>
/// <param name="Source">The source of questions.</param>
/// <param name="DataLoader">List of question loaders. </param>
/// <returns>A function that loads questions from a config loader.</returns>
/// <example><![CDATA[var cfg = new QuestionConfig("csv", loader => loader.Load());]]></example>
public record QuestionConfig(string Source, Func<IConfigurationLoader, IEnumerable<Question>> DataLoader);
