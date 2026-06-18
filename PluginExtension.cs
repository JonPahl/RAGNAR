namespace Ragnar;

/// <summary>
/// Extension methods for plugin-related operations.
/// </summary>
public static class PluginExtension
{
    extension(List<QuestionConfiguration> configs)
    {
        /// <summary>
        /// Loads questions from a plugin configuration file.
        /// </summary>
        /// <param name="name">File name to read.</param>
        /// <param name="provider">Plugin configuration.</param>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Loaded question configurations.</returns>
        /// <example><![CDATA[List<QuestionConfiguration> configs = await LoadPluginQuestionsAsync("General.csv", provider, ct);]]></example>
        public async Task<List<QuestionConfiguration>> LoadPluginQuestionsAsync(string name, IQuestionProvider provider, CancellationToken ct)
        {
            provider.SetFileName(name);
            var questions = await provider.LoadQuestionAsync(ct);
            configs.AddRange(questions);
            return configs;
        }
    }
}
