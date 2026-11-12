namespace Ragnar.Store;

/// <summary>
/// A JSON-file-backed fingerprint store that survives across runs.
/// </summary>
/// <param name="logger">Serilog logger for diagnostics.</param>
/// <param name="option"></param>
/// <example><![CDATA[var store = new JsonFileFingerprintStore(log, Path.Combine(root, ".ragnar"));]]></example>
public sealed class JsonFileFingerprintStore(
    ILogger logger,
    IOptions<RagnarConfig> option) : IFileFingerprintStore
{
    /// <summary> The lock used to synchronize access to the fingerprint store. </summary>
    private readonly Lock _gate = new();

    /// <summary>
    /// A dictionary of fingerprints where each key is a file path and each value is its corresponding SHA-256 hash.
    /// </summary>
    private Dictionary<string, string> _fingerprints = [];

    /// <summary>
    /// The file path to the fingerprint store on disk.
    /// </summary>
    private string _filePath = string.Empty;

    /// <summary> A counter of dirty fingerprints that need to be flushed to disk. </summary>
    private int _dirtyCount;

    /// <summary> The threshold for flushing pending fingerprints to disk (256). </summary>
    private const int _flushThreshold = 256;

    private readonly JsonSerializerOptions _jsonOption = new() { WriteIndented = true };

    /// <summary>
    /// Loads or creates the fingerprint file from disk.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the fingerprint file is loaded or created.</returns>
    /// <example><![CDATA[await store.InitializeAsync(ct);]]></example>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var basePath = option.Value.ApplicationOptions.SourceDirectory;
        Directory.CreateDirectory(basePath);
        _filePath = Path.Combine(basePath, "fingerprints.json");
        if (File.Exists(_filePath))
        {
            var json = await File.ReadAllTextAsync(_filePath, cancellationToken).ConfigureAwait(false);
            _fingerprints = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
            logger.LogInformation("Loaded {Count} existing fingerprints from {Path}", _fingerprints.Count, _filePath);
        }
        else
        {
            logger.LogInformation("No existing fingerprint file at {Path}; starting fresh", _filePath);
        }
    }

    /// <summary>Checks if a file's fingerprint has changed.</summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="sha256">The expected SHA-256 hash of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the stored hash matches; <c>false</c> otherwise.</returns>
    /// <example><![CDATA[bool same = await store.IsUnchangedAsync(path, hash, ct);]]></example>
    public async Task<bool> IsUnchangedAsync(string filePath, string sha256, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        bool result;
        lock (_gate)
            result = _fingerprints.TryGetValue(filePath, out var stored) && stored == sha256;

        await Task.Yield();
        return result;
    }

    /// <summary>Records a file's fingerprint. </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="sha256">The SHA-256 hash of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the fingerprint is recorded.</returns>
    public async Task RecordAsync(string filePath, string sha256, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        bool shouldFlush;
        lock (_gate)
        {
            _fingerprints[filePath] = sha256;
            shouldFlush = ++_dirtyCount >= _flushThreshold;
            if (shouldFlush) _dirtyCount = 0;
        }
        if (shouldFlush)
        {
            await PersistAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Flushes any pending fingerprints to disk.</summary>
    /// <param name="cancellationToken">Token to cancel the flush operation.</param>
    /// <returns>A task that completes when pending fingerprints are persisted.</returns>
    /// <example><![CDATA[await store.FlushAsync(ct);]]></example>
    public async Task FlushAsync(CancellationToken cancellationToken)
    {
        bool needed;
        lock (_gate)
        {
            needed = _dirtyCount > 0;
            _dirtyCount = 0;
        }
        if (needed)
            await PersistAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Removes a file's fingerprint.</summary>
    /// <param name="filePath">The path of the file whose fingerprint is removed.</param>
    /// <param name="cancellationToken">Token to cancel the removal operation.</param>
    /// <returns>A task that completes when the fingerprint is removed and persisted.</returns>
    /// <example><![CDATA[await store.RemoveAsync(path, ct);]]></example>
    public async Task RemoveAsync(string filePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
            _fingerprints.Remove(filePath);
        await PersistAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Serializes the current fingerprint dictionary and writes it to disk.</summary>
    /// <param name="cancellationToken">Token to cancel the write operation.</param>
    /// <returns>A task that completes when the JSON file has been written.</returns>
    /// <example><![CDATA[await store.PersistAsync(ct);]]></example>
    private async Task PersistAsync(CancellationToken cancellationToken)
    {
        string snapshot;

        lock (_gate)
            snapshot = JsonSerializer.Serialize(_fingerprints, _jsonOption);
        await File.WriteAllTextAsync(_filePath, snapshot, cancellationToken).ConfigureAwait(false);
    }
}
