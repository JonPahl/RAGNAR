namespace Ragnar.Core.Models;

/// <summary>Shared context that flows through the question-execution pipeline.</summary>
/// <example><![CDATA[await runner.ExecuteAsync(new QuestionPipelineContext(), ct);]]></example>
public class QuestionPipelineContext
{
    private IReadOnlyList<Question>? _questions;

    /// <summary>Loaded and filtered questions (populated by <see cref="QuestionLoadStage"/>).</summary>
    /// <returns>A read-only list of loaded questions, or null before load.</returns>
    /// <example><![CDATA[var q = ctx.Questions;]]></example>
    public IReadOnlyList<Question>? Questions => _questions;

    /// <summary>Called exclusively by <see cref="QuestionLoadStage"/>. </summary>
    /// <param name="questions">The filtered list of questions to store.</param>
    /// <returns>Void; mutates internal state.</returns>
    /// <example><![CDATA[ctx.SetLoadedQuestions(loadedQuestions);]]></example>
    public void SetLoadedQuestions(IReadOnlyList<Question> questions) => _questions = questions;

    /// <summary>Accumulated LLM responses keyed by question ID.</summary>
    /// <returns>Dictionary of LLM answers keyed by question GUID.</returns>
    /// <example><![CDATA[string a = ctx.Answers[[qid]];]]></example>
    public ConcurrentDictionary<Guid, string> Answers { get; } = new();

    /// <summary>Per-question wall-clock timings.</summary>
    /// <returns>Dictionary of per-question execution durations keyed by GUID.</returns>
    /// <example><![CDATA[TimeSpan t = ctx.QuestionTimings[qid];]]>
    /// </example>
    public ConcurrentDictionary<Guid, TimeSpan> QuestionTimings { get; } = new();
}
