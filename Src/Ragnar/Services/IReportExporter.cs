namespace Ragnar.Services;

/// <summary> Exports RAG session results and summaries to disk in a chosen format. </summary>
/// <example><![CDATA[
/// await exporter.ExportAsync(session, ReportFormat.Markdown, "./reports/session.md");
/// ]]></example>
public interface IReportExporter
{
    /// <summary>
    /// Exports the given session (or batch results) to a file.
    /// </summary>
    /// <param name="results">The collection of RAG responses to include.</param>
    /// <param name="format">Target output format.</param>
    /// <param name="outputPath">Destination file path (directory auto-created).</param>
    /// <param name="cancellationToken">Token to cancel the write.</param>
    /// <returns>The fully qualified path of the written file.</returns>
    /// <example><![CDATA[await exporter.ExportAsync(results, ReportFormat.Markdown, "./out.md");]]></example>
    Task<string> ExportAsync(
        IReadOnlyList<RagResponse> results,
        ReportFormat format,
        string outputPath,
        CancellationToken cancellationToken = default);
}

/// <summary>Supported report output formats.</summary>
public enum ReportFormat { Markdown, Html, Json }