namespace Ragnar.Core.Model;

/// <summary>Mutable state passed between embedding pipeline stages.</summary>
/// <remarks>
/// Populated incrementally: Discovery → Parsing → Upsert.
/// Each stage reads the fields it needs and writes its output before yielding.
/// </remarks>
public class EmbeddingContext
{
    /// <summary>Resolved source directory (set by configuration stage).</summary>
    public string SourceDirectory { get; set; } = null!;

    /// <summary>Discovered file paths (populated by DiscoveryStage).</summary>
    public IReadOnlyList<string> DiscoveredFiles { get; set; } = [];

    /// <summary>Parsed code documents (populated by ParsingStage).</summary>
    public IReadOnlyList<CodeDocument> Documents { get; set; } = [];

    /// <summary>Upsert result from the final stage.</summary>
    public UpdateResult? UpsertResult { get; set; }

    /// <summary>Elapsed time across all stages (set by orchestrator).</summary>
    public TimeSpan TotalElapsed { get; set; }

    /// <summary>Optional category filter carried through the pipeline.</summary>
    public QuestionCategory? CategoryFilter { get; init; }
}
