using Ragnar.Core.Model;

namespace Ragnar.Core.Interface;

public interface IRagOrchestrator
{
    /// <summary>Executes the RAG pipeline for a question.</summary>
    /// <param name="question">User query.</param>
    /// <param name="contextText">Retrieved context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Async task.</returns>
    /// <example><![CDATA[await pipeline.RunAsync(q, ctx, ct);]]></example>
    Task RunAsync(Question question, string contextText, CancellationToken ct);
}
