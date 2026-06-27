namespace RAGNAR.OutputResponse;

/// <summary>
/// Provides a service to summarize all responses.
/// </summary>
public interface ISummaryService
{
    /// <summary>
    /// Asynchronously summarizes all responses.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A task containing the summarized responses.</returns>
    ValueTask SummarizeAllResponsesAsync(CancellationToken ct);
}
