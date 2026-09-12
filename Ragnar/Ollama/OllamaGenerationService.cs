namespace Ragnar.Ollama;

/// <summary>
/// Configures and caches OllamaOptions clients per model.
/// </summary>
/// <example><![CDATA[var provider = new OllamaGenerationService(opts);]]></example>
/// <remarks>
/// Initializes a new instance of the <see cref="OllamaGenerationService"/> class.
/// Template Ollama API call.
/// </remarks>
/// <param name="clientFactory">Ollama setup factory.</param>
public class OllamaGenerationService(IOllamaClientFactory clientFactory)
    : IOllamaGenerationService
{
    private readonly OllamaApiClient _ollamaClient = clientFactory.FindClient(OllamaServiceType.Ollama);

    /// <summary>Streams and collects full LLM response into a string.</summary>
    /// <param name="request">Generation request with prompt/options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Full generated text.</returns>
    /// <example><![CDATA[string answer = await provider.GenerateResponse(request, ct);]]></example>
    public async Task<string> GenerateResponse(
        GenerateRequest request,
        CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        try
        {
            request.Options = new()
            {
                Temperature = 0.2f,
                RepeatPenalty = 1.02f,
            };

            //TODO: Rework to use chatClient
            //Also include tracking on thinking vs finished results.

            var panelText = new Markup(string.Empty, Styles.Yellow).LeftJustified();

            var headerText = " Generating... ";

            var panel = new Panel(panelText)
                .Header(headerText)
                .BorderColor(Color.Green)
                .RoundedBorder()
                .BorderStyle(Styles.GreenBlink)
                .Expand()
                .Padding(1, 1, 1, 1);

            await AnsiConsole
                .Live(panel)
                .StartAsync(async ctx =>
            {
                ctx.UpdateTarget(panel);
                ctx.Refresh();

                await foreach (var stream in _ollamaClient.GenerateAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    if (stream is null)
                        throw new InvalidOperationException("Stream returned null response.");

                    sb.Append(stream.Response.AsSpan());

                    panel.BorderStyle = null;

                    panelText = new Markup(sb.ToString().EscapeMarkup(), Styles.Yellow);

                    headerText = " Streaming Response ";

                    ctx.UpdateTarget(panel);
                    ctx.UpdateTarget(panelText);
                    ctx.Refresh();
                }
            }).ConfigureAwait(false);

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
