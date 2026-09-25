namespace Ragnar.Core.Model;

/// <summary>Shared context that flows through the question-execution pipeline.</summary>
public class QuestionPipelineContext
{
    /// <summary>Loaded and filtered questions (populated by <see cref="QuestionLoadStage"/>).</summary>
    public IReadOnlyList<Question>? Questions { get; set; }

    /// <summary>Accumulated LLM responses keyed by question ID.</summary>
    public ConcurrentDictionary<Guid, string> Responses { get; } = new();

    /// <summary>Per-question wall-clock timings.</summary>
    public ConcurrentDictionary<Guid, TimeSpan> Timings { get; } = new();
}
