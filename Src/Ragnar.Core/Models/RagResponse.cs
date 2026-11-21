namespace Ragnar.Core.Models;

/// <summary>Immutable result of a single RAG query within a session.</summary>
/// <param name="Question">The user's question text.</param>
/// <param name="Answer">The LLM-generated answer.</param>
/// <param name="RetrievedContext">The raw context chunks used for grounding.</param>
/// <param name="Confidence">Heuristic confidence score in [0, 1].</param>
/// <param name="RetrievalDurationMs">Time taken for the vector search, in milliseconds.</param>
/// <param name="Category">Optional category filter that was applied.</param>
public sealed record RagResponse(
    string Question,
    string Answer,
    string RetrievedContext,
    double Confidence,
    long RetrievalDurationMs,
    QuestionCategory? Category = null);

