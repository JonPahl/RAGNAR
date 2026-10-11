namespace Ragnar.Questions.Interfaces;

/// <summary>
/// Gets questions from a configuration location.
/// </summary>
public interface IConfigurationLoader
{
    /// <summary>
    /// Gets questions from a configuration file.
    /// </summary>
    /// <returns>Collection of Question objects.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the configuration file is not found.</exception>
    /// <example><![CDATA[loader.LoadQuestions();]]>
    /// </example>
    IEnumerable<Question> LoadQuestions();
}
