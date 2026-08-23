namespace Ragnar.Core.Interface;

public interface IRagOrchestrator
{
    /// <summary>Executes the RAG pipeline for a Question.</summary>
    /// <param name="Question">User query.</param>
    /// <param name="ContextText">Retrieved context.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Async task.</returns>
    /// <example><![CDATA[await pipeline.ExecuteAsync(q, ctx, ct);]]></example>
    Task ExecuteAsync(Question Question, string ContextText, CancellationToken Ct);
}
