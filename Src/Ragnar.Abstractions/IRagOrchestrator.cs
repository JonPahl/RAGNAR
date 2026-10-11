namespace Ragnar.Abstractions;

/// <summary>Defines the contract for orchestrating the RAG question-to-answer pipeline.</summary>
/// <remarks>Implemented by RagOrchestrator to embed, retrieve, and generate responses.</remarks>
/// <example><![CDATA[var p = sp.GetRequiredService<IRagOrchestrator>();]]></example>
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
