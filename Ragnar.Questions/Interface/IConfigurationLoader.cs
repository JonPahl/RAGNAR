namespace Ragnar.Questions.Interface;

/// <summary>
/// Gets questions from a configuration location.
/// </summary>
public interface IConfigurationLoader
{
    /// <summary>
    /// Gets questions from a configuration file.
    /// </summary>
    /// <example>
    /// <![CDATA[
    /// var loader = new IConfigurationLoader();
    /// loader.LoadQuestions(); ]]>
    /// </example>
    /// <returns>Collection of Question objects.
    /// </returns>
    IEnumerable<Plugins.Question> LoadQuestions();
}
