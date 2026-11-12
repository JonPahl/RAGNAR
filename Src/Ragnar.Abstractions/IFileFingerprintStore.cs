namespace Ragnar.Abstractions;

/// <summary>Persists file-content fingerprints so unchanged files are skipped on re-embed.</summary>
/// <example><![CDATA[if (await store.IsUnchangedAsync(path, hash)) skip;]]></example>
public interface IFileFingerprintStore
{
    /// <summary>Initializes the fingerprint store.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Completed task once initialization is complete.</returns>
    Task InitializeAsync(CancellationToken cancellationToken);

    /// <summary>Returns true when the file's hash matches the last stored value.</summary>
    /// <param name="filePath">Absolute or relative file path used as the key.</param>
    /// <param name="sha256">Current SHA-256 hex digest of the file content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the file has not changed since the last successful embed.</returns>
    /// <example><![CDATA[bool skip = await store.IsUnchangedAsync("src/App.cs", "ab3f…");]]></example>
    Task<bool> IsUnchangedAsync(string filePath, string sha256, CancellationToken cancellationToken);

    /// <summary>Records a new fingerprint after a successful upsert.</summary>
    /// <param name="filePath">File path key.</param>
    /// <param name="sha256">Hash to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <example><![CDATA[await store.RecordAsync("src/App.cs", "ab3f…", ct);]]></example>
    Task RecordAsync(string filePath, string sha256, CancellationToken cancellationToken);

    /// <summary>Removes the fingerprint (e.g. after a file is deleted from source).</summary>
    /// <param name="filePath">File path key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <example><![CDATA[await store.RemoveAsync("src/Deleted.cs", ct);]]></example>
    Task RemoveAsync(string filePath, CancellationToken cancellationToken);

    Task FlushAsync(CancellationToken cancellationToken);
}
