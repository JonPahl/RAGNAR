namespace Ragnar.Services;

/// <summary>
/// Stateful RAG conversation session with multi-turn context and confidence scoring. </summary>
/// <param name="retriever">Service that performs similarity search against Qdrant.</param>
/// <param name="orchestrator">Service that composes prompts and invokes the LLM.</param>
/// <param name="options">Application configuration (vector store name, top-k, etc.).</param>
/// <param name="logger">Structured logger for diagnostic messages.</param>
/// <param name="writer">Console output writer for interactive rendering.</param>
public sealed class RagSession(
    IContextRetriever retriever,
    IRagOrchestrator orchestrator,
    IOptions<RagnarConfig> options,
    ILogger<RagSession> logger,
    IOutputWriter writer) : IRagSession
{
    private readonly List<ChatMessage> _history = [];
    private string? _collectionName;
    private int _maxTurns;
    private int _turnCount;

    /// <inheritdoc />
    public bool IsActive => _collectionName is not null;

    /// <inheritdoc />
    public IReadOnlyList<ChatMessage> History => _history.AsReadOnly();

    /// <inheritdoc />
    public Task BeginAsync(string vectorStoreName, int maxTurns = 10, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vectorStoreName);
        if (maxTurns < 1) throw new ArgumentOutOfRangeException(nameof(maxTurns), maxTurns, "Must be ≥ 1.");

        _collectionName = vectorStoreName;
        _maxTurns = maxTurns;
        _turnCount = 0;
        _history.Clear();

        // Seed the system message so the LLM knows its role.
        _history.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.System,
            "You are Ragnar, a code-analysis assistant. Answer questions using ONLY the provided " +
            "retrieved context. If the context is insufficient, say so explicitly. Cite the source " +
            "file when referencing code. Keep responses under 400 words unless the user requests detail."));

        logger.LogInformation("Session started: collection='{Col}', maxTurns={MT}", vectorStoreName, maxTurns);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<RagResponse> AskAsync(string question, QuestionCategory? category = null, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(question);
        if (!IsActive) throw new InvalidOperationException("Session is not active. Call BeginAsync first.");

        cancellationToken.ThrowIfCancellationRequested();
        _turnCount++;

        // 1. Build the retrieval filter for the current question.
        var filter = category.HasValue ? BuildCategoryFilter(category.Value) : null;

        // 2. Retrieve relevant document chunks.
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var context = await retriever
            .RetrieveContextAsync(_collectionName!, cancellationToken, filter)
            .ConfigureAwait(false);
        var retrievalMs = stopwatch.ElapsedMilliseconds;

        logger.LogDebug("Retrieved {Chars} chars of context in {Ms} ms", context.Length, retrievalMs);

        // 3. Compose the full prompt with history for multi-turn awareness.
        var prompt = BuildPromptWithHistory(question, context);

        // 4. Call the LLM via the orchestrator.
        var answer = await orchestrator
            .GenerateAsync(prompt, cancellationToken)
            .ConfigureAwait(false);

        // 5. Compute a simple confidence proxy based on retrieval length vs. question length.
        var confidence = ComputeConfidence(context, question);

        // 6. Record the turn.
        _history.Add(new ChatMessage(
            Microsoft.Extensions.AI.ChatRole.User, question));
        _history.Add(new ChatMessage(
            Microsoft.Extensions.AI.ChatRole.Assistant, answer));
        TrimHistory();

        return new RagResponse(
            question, answer, context, confidence, retrievalMs, category);
    }

    /// <inheritdoc />
    public Task EndAsync(string? exportPath = null, CancellationToken cancellationToken = default)
    {
        if (!IsActive) return Task.CompletedTask;

        if (exportPath is not null)
        {
            ExportTranscript(exportPath);
            logger.LogInformation("Transcript exported to {Path}", exportPath);
        }

        _collectionName = null;
        _history.Clear();
        _turnCount = 0;
        return Task.CompletedTask;
    }

    private string BuildPromptWithHistory(string question, string context)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("## Retrieved Context");
        sb.AppendLine(context);
        sb.AppendLine();

        // Append recent turns (last 3) for conversational context.
        var recentTurns = _history.Where(m => m.Role != Microsoft.Extensions.AI.ChatRole.System).ToList();
        if (recentTurns.Count > 0)
        {
            sb.AppendLine("## Previous Conversation");
            foreach (var msg in recentTurns.TakeLast(6))
            {
                sb.AppendLine($"{msg.Role}: {msg.Text}");
            }

            sb.AppendLine();
        }

        sb.AppendLine("## Current Question");
        sb.AppendLine(question);
        return sb.ToString();
    }

    private void TrimHistory()
    {
        // Keep system message + last N*2 messages.
        var maxMessages = (_maxTurns * 2) + 1; // +1 for system
        while (_history.Count > maxMessages)
        {
            // Remove the oldest non-system message.
            var idx = _history.FindIndex(m => m.Role != Microsoft.Extensions.AI.ChatRole.System);
            if (idx == -1) break;
            _history.RemoveAt(idx);
        }
    }

    private static double ComputeConfidence(string context, string question)
    {
        // Heuristic: ratio of unique word overlap between question and context.
        if (string.IsNullOrWhiteSpace(context)) return 0.0;
        var qWords = new HashSet<string>(question.Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
        var cWords = new HashSet<string>(context.Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
        var overlap = qWords.Count(w => cWords.Contains(w));
        return Math.Min(1.0, (double)overlap / Math.Max(1, qWords.Count));
    }

    private Filter BuildCategoryFilter(QuestionCategory category)
    {
        var filterBuilder = new QdrantFilterBuilder()
            .AddMustMatch("Category", category.ToString());
        return filterBuilder.Build();

        //=> new(FieldCondition(Key: "Category", Match: new MatchValue(value: category.ToString())));
    }

    private void ExportTranscript(string path)
    {
        var dir = System.IO.Path.GetDirectoryName(path);
        if (dir is not null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

        using var sw = new StreamWriter(path);
        sw.WriteLine("# Ragnar Session Transcript");
        sw.WriteLine($"Generated: {DateTime.UtcNow:O}");
        sw.WriteLine();
        foreach (var msg in History)
        {
            sw.WriteLine($"### {msg.Role}");
            sw.WriteLine(msg.Text);
            sw.WriteLine();
        }
    }
}
