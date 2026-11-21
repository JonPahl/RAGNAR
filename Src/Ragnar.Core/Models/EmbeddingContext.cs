namespace Ragnar.Core.Models;

/// <summary>Mutable state passed between embedding pipeline stages.</summary>
/// <remarks>
/// Populated incrementally: Discovery → Parsing → Upsert.
/// Each stage reads the fields it needs and writes its output before yielding. </remarks>
public class EmbeddingContext
{
    /// <summary>Resolved source directory (set by configuration stage).</summary>
    public string SourceDirectory { get; set; } = string.Empty;

    /// <summary>Discovered file paths (populated by DiscoveryStage).</summary>
    /// <example><![CDATA[var files = ctx.DiscoveredFiles;]]></example>
    public IReadOnlyList<string> DiscoveredFiles { get; set; } = [];

    /// <summary>Parsed code documents (populated by ParsingStage).</summary>
    /// <example><![CDATA[var docs = ctx.Documents;]]></example>
    public IReadOnlyList<CodeDocument> Documents { get; set; } = [];

    /// <summary>Upsert result from the final stage.</summary>
    /// <example><![CDATA[UpdateResult? r = ctx.UpsertResult;]]></example>
    public UpdateResult? UpsertResult { get; set; }

    /// <summary>Elapsed time across all stages (set by orchestrator).</summary>
    /// <example><![CDATA[TimeSpan t = ctx.TotalElapsed;]]></example>
    public TimeSpan TotalElapsed { get; set; }

    /// <summary>Optional question category to scope pipeline processing.</summary>
    /// <example><![CDATA[ctx.CategoryFilter = QuestionCategory.Refactor;]></example>
    public QuestionCategory? CategoryFilter { get; init; }

    /// <summary>Number of files skipped during the pipeline run.</summary>
    /// <example><![CDATA[int n = ctx.SkippedCount; // 3]></example>
    public int SkippedCount { get; set; }
}
