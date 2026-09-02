namespace Ragnar.Plugins;

/// <summary>Provides questions from an external source.</summary>
public interface IQuestionProvider
{
    /// <summary>Gets the name of this provider (e.g., "JSON", "API").</summary>
    string ProviderName { get; }

    /// <summary>Lets the provider load and return a set of question configurations asynchronously.</summary>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>A task that yields an enumerable of question configurations.</returns>
    /// <remarks>Implementations should handle cancellation gracefully.</remarks>
    /// <example><![CDATA[
    /// var provider = new JsonQuestionProvider("questions.json");
    /// var questions = await provider.LoadQuestionsAsync(ct);
    /// ]]></example>
    Task<IEnumerable<Question>> LoadQuestionsAsync(string fileName, CancellationToken cancellationToken);
}
