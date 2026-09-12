namespace Ragnar.Extensions;

/// <summary>
/// Extension methods for plugin-related operations.
/// </summary>
public static class PluginExtension
{
    extension(List<Plugins.Question> configs)
    {
        /// <summary>
        /// Loads questions from a plugin configuration file.
        /// </summary>
        /// <param name="name">File name to read.</param>
        /// <param name="provider">Plugin configuration.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>Loaded question configurations.</returns>
        /// <example><![CDATA[List<Question> configs = await LoadPluginQuestionsAsync("General.csv", provider, ct);]]></example>
        public async Task<List<Plugins.Question>> LoadPluginQuestionsAsync(string name, IQuestionProvider provider, CancellationToken cancellationToken)
        {
            var questions = await provider.LoadQuestionsAsync(name, cancellationToken).ConfigureAwait(false);
            configs.AddRange(questions);
            return configs;
        }
    }
}
