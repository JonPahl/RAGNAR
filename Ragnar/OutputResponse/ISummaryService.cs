namespace Ragnar.OutputResponse;

/// <summary>
/// Provides a service to summarize all responses.
/// </summary>
/// <example><![CDATA[await svc.SummarizeAllResponsesAsync(ct);]]></example>
public interface ISummaryService
{
    /// <summary>
    /// Asynchronously summarizes all responses.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task containing the summarized responses.</returns>
    /// <example><![CDATA[await svc.SummarizeAllResponsesAsync(ct);]]></example>
    ValueTask SummarizeAllResponsesAsync(CancellationToken cancellationToken);
}
