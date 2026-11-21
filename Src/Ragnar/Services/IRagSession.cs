namespace Ragnar.Services;

    public interface IRagSession
    {
        /// <summary>Gets whether the session is currently active.</summary>
        bool IsActive { get; }

        /// <summary>Gets the current conversation history (system + user + assistant turns).</summary>
        IReadOnlyList<ChatMessage> History { get; }

        /// <summary>
        /// Begins a new session, loading the vector collection metadata and resetting history.
        /// </summary>
        /// <param name="vectorStoreName">Name of the Qdrant collection to query.</param>
        /// <param name="maxTurns">Maximum number of user/assistant turns before history is trimmed.</param>
        /// <param name="cancellationToken">Token to cancel initialization.</param>
        Task BeginAsync(string vectorStoreName, int maxTurns = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sends a user question, retrieves relevant context, and generates an LLM response.
        /// </summary>
        /// <param name="question">The natural-language question.</param>
        /// <param name="category">Optional category filter to narrow retrieval.</param>
        /// <param name="cancellationToken">Token to cancel the request.</param>
        /// <returns>A <see cref="RagResponse"/> containing the answer, retrieved context, and confidence score.</returns>
        /// <example><![CDATA[
        /// var response = await session.AskAsync("How is cancellation handled?", ct);
        /// Console.WriteLine(response.Answer);
        /// Console.WriteLine($"Confidence: {response.Confidence:P1}");
        /// ]]></example>
        Task<RagResponse> AskAsync(string question, QuestionCategory? category = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ends the session, optionally persisting the transcript to disk.
        /// </summary>
        /// <param name="exportPath">Optional file path to write the transcript. Null skips export.</param>
        /// <param name="cancellationToken">Token to cancel the export.</param>
        Task EndAsync(string? exportPath = null, CancellationToken cancellationToken = default);
    }
