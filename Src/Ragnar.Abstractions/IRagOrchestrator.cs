namespace Ragnar.Abstractions;

/// <summary>Orchestrates the full RAG pipeline: prompt building, Ollama generation, and response persistence.</summary>
/// <example><![CDATA[await pipeline.ExecuteAsync(question, ctx, ct);]]></example>
public interface IRagOrchestrator
{
    /// <summary>Executes the RAG pipeline for a Question.</summary>
    /// <param name="question">User query.</param>
    /// <param name="contextText">Retrieved context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async task.</returns>
    /// <example><![CDATA[await pipeline.ExecuteAsync(q, ctx, ct);]]></example>
    Task ExecuteAsync(Question question, string contextText, CancellationToken cancellationToken);
}
