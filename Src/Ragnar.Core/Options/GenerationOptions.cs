namespace Ragnar.Core.Options;

/// <summary>
/// Immutable options controlling LLM generation parameters.
/// </summary>
/// <example><![CDATA[var opts = new GenerationOptions { MaxTokens = 4096 };]]>
/// </example>
public record GenerationOptions
{
    /// <summary>Gets the maximum number of tokens to generate per response.</summary>
    /// <returns>Upper token limit; defaults to 32 768.</returns>
    /// <example><![CDATA[int max = opts.MaxTokens; // 32768]]></example>
    public int MaxTokens { get; init; } = 32_768;

    /// <summary>Gets the model context window size in tokens.</summary>
    /// <returns>Context window token count; defaults to 65 536.</returns>
    /// <example><![CDATA[int ctx = opts.ContextWindow; // 65536]]></example>
    public int ContextWindow { get; init; } = 65_536;

    /// <summary>Gets the maximum concurrent embedding threads.</summary>
    /// <returns>Thread pool size; defaults to 8.</returns>
    /// <example><![CDATA[int t = opts.ThreadCount; // 8]]></example>
    public int ThreadCount { get; init; } = 8;

    /// <summary>Gets the sampling temperature (0.0 – 2.0).</summary>
    /// <returns>Temperature value; defaults to 0.2 for deterministic output.</returns>
    /// <example><![CDATA[float temp = opts.Temperature; // 0.2f]]></example>
    public float Temperature { get; init; } = 0.2f;

    /// <summary>Gets the repetition penalty applied to token logits.</summary>
    /// <returns>Penalty multiplier; defaults to 1.02.</returns>
    /// <example><![CDATA[float p = opts.RepeatPenalty; // 1.02f]]></example>
    public float RepeatPenalty { get; init; } = 1.02f;
}
