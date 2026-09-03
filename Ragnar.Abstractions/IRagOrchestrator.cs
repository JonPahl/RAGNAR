namespace Ragnar.Abstractions;

public interface IRagOrchestrator
{
    /// <summary>Executes the RAG pipeline for a Question.</summary>
    /// <param name="question">User query.</param>
    /// <param name="contextText">Retrieved context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async task.</returns>
    /// <example><![CDATA[await pipeline.ExecuteAsync(q, ctx, ct);]]></example>
    Task ExecuteAsync(Core.Model.Question question, string contextText, CancellationToken cancellationToken);
}
