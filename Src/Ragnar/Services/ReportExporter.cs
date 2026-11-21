using System.Text.Json.Serialization;

namespace Ragnar.Services;

/// <summary>
/// Renders RAG results into Markdown, HTML, or JSON report files.
/// </summary>
/// <param name="logger">Logger for export diagnostics.</param>
/// <param name="options">Application options supplying the app name and version for the report header.</param>
public sealed class ReportExporter(ILogger<ReportExporter> logger, IOptions<RagnarConfig> options) : IReportExporter
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <inheritdoc />
    public Task<string> ExportAsync(
        IReadOnlyList<RagResponse> results,
        ReportFormat format,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(results);
        Guard.Against.NullOrWhiteSpace(outputPath);

        cancellationToken.ThrowIfCancellationRequested();

        var dir = System.IO.Path.GetDirectoryName(outputPath);
        if (dir is not null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var content = format switch
        {
            ReportFormat.Markdown => RenderMarkdown(results),
            ReportFormat.Html => RenderHtml(results),
            ReportFormat.Json => RenderJson(results),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };

        File.WriteAllText(outputPath, content);
        logger.LogInformation("Report written: {Path} ({Format}, {Count} entries)", outputPath, format, results.Count);
        return Task.FromResult(Path.GetFullPath(outputPath));
    }

    // ───────────────────────── Renderers ─────────────────────────

    private string RenderMarkdown(IReadOnlyList<RagResponse> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Ragnar Analysis Report");
        sb.AppendLine();
        sb.AppendLine("| | |");
        sb.AppendLine("|---|---|");
        sb.AppendLine("|**Generated**|{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC|");
        sb.AppendLine($"|**Questions**|{results.Count}|");
        sb.AppendLine($"|**Avg Confidence|{results.Average(r => r.Confidence):P1}|");
        sb.AppendLine();

        for (var i = 0; i < results.Count; i++)
        {
            var r = results[i];
            sb.AppendLine($"## {i + 1}. {r.Question}");
            sb.AppendLine();
            sb.AppendLine($"- **Category:** {r.Category?.ToString() ?? "General"}");
            sb.AppendLine($"- **Confidence:** {r.Confidence:P1}");
            sb.AppendLine($"- **Retrieval time:** {r.RetrievalDurationMs} ms");
            sb.AppendLine();
            sb.AppendLine("### Answer");
            sb.AppendLine();
            sb.AppendLine(r.Answer);
            sb.AppendLine();
            sb.AppendLine("<details>");
            sb.AppendLine("<summary>Retrieved context</summary>");
            sb.AppendLine();
            sb.AppendLine("```");
            sb.AppendLine(r.RetrievedContext);
            sb.AppendLine("```");
            sb.AppendLine("</details>");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string RenderHtml(IReadOnlyList<RagResponse> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\">");
        sb.AppendLine($"<title>Ragnar Report – {DateTime.UtcNow:yyyy-MM-dd}</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body{font-family:system-ui,sans-serif;max-width:860px;margin:2rem auto;padding:0 1rem;line-height:1.6}");
        sb.AppendLine(".question{border-left:4px solid #4a9eff;padding-left:1rem;margin:2rem 0}");
        sb.AppendLine(".confidence{display:inline-block;padding:2px 8px;border-radius:12px;font-size:.85rem}");
        sb.AppendLine(".high{background:#d4edda;color:#155724}.mid{background:#fff3cd;color:#856404}.low{background:#f8d7da;color:#721c24}");
        sb.AppendLine("pre{background:#f4f4f4;padding:1rem;border-radius:6px;overflow-x:auto}");
        sb.AppendLine("details{margin:.5rem 0}");
        sb.AppendLine("</style></head><body>");
        sb.AppendLine($"<h1>Ragnar Analysis Report</h1>");
        sb.AppendLine($"<p>Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC · {results.Count} question(s)</p>");

        foreach (var r in results)
        {
            var level = r.Confidence >= 0.7 ? "high" : r.Confidence >= 0.4 ? "mid" : "low";
            sb.AppendLine("<div class=\"question\">");
            sb.AppendLine("<h2>{System.Net.WebUtility.HtmlEncode(r.Question)}</h2>");
            sb.AppendLine($"<span class=\"confidence {level}\">{r.Confidence:P1}</span> " +
                          $"<small>{r.Category?.ToString() ?? "General"} · {r.RetrievalDurationMs} ms</small>");
            sb.AppendLine($"<p>{System.Net.WebUtility.HtmlEncode(r.Answer)}</p>");
            sb.AppendLine($"<details><summary>Context</summary><pre>{System.Net.WebUtility.HtmlEncode(r.RetrievedContext)}</pre></details>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }

    private static string RenderJson(IReadOnlyList<RagResponse> results)
        => JsonSerializer.Serialize(results, JsonOpts);
}
