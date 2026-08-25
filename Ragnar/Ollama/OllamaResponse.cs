namespace Ragnar.Ollama;

/// <summary>
/// Configures and caches OllamaOptions clients per model.
/// </summary>
/// <example><![CDATA[var provider = new OllamaResponse(opts);]]></example>
/// <remarks>
/// Initializes a new instance of the <see cref="OllamaResponse"/> class.
/// Template Ollama api call.
/// </remarks>
/// <param name="ClientFactory">Ollama setup factory.</param>
public class OllamaResponse(IOllamaClientFactory ClientFactory, IOptions<RagnarConfig> Options) : IOllamaResponse
{
    private readonly OllamaApiClient _ollamaClient = ClientFactory.FindClient(OllamaServiceType.Ollama);

    /// <summary>Streams and collects full LLM response into a string.</summary>
    /// <param name="Request">Generation request with prompt/options.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Full generated text.</returns>
    /// <example><![CDATA[string answer = await provider.GenerateResponse(request, ct);]]></example>
    public async Task<string> GenerateResponse(
        GenerateRequest Request,
        CancellationToken Ct)
    {
        var sb = new StringBuilder();

        try
        {
            Request.Options = new()
            {
                Temperature = 0.2f,
                RepeatPenalty = 1.02f,
            };

            sb = new StringBuilder();

            var panelText = new Markup(string.Empty, Styles.Yellow).LeftJustified();

            var headerText = " Generating... ";

            var panel = new Panel(panelText)
                .Header(headerText)
                .BorderColor(Color.Green)
                .RoundedBorder()
                .BorderStyle(Styles.GreenBlink)
                .Expand()
                .Padding(1, 1, 1, 1);

            await AnsiConsole.Live(panel)
                .StartAsync(async ctx =>
            {
                ctx.UpdateTarget(panel);
                ctx.Refresh();

                await foreach (var stream in _ollamaClient.GenerateAsync(Request, Ct))
                {
                    if (stream is null)
                        throw new InvalidOperationException("Stream returned null response.");

                    sb.Append(stream.Response.AsSpan());

                    if (!string.IsNullOrWhiteSpace(stream.Response))
                    {
                        panel.BorderStyle = null;

                        panelText = new Markup(sb.ToString().EscapeMarkup(), Styles.Yellow);

                        headerText = " Streaming Response ";

                        ctx.UpdateTarget(panelText);
                        ctx.Refresh();
                    }
                }
            });

            return sb.ToString();
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return ex.Message;
        }
        finally
        {
            sb.Clear();
        }
    }
}
