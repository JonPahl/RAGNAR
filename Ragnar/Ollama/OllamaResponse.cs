namespace Ragnar.Ollama;

/// <summary>
/// Configures and caches OllamaOptions clients per model.
/// </summary>
/// <example><![CDATA[var provider = new OllamaResponse(opts);]]></example>
public class OllamaResponse : IOllamaResponse
{
    private readonly OllamaApiClient _ollamaClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaResponse"/> class.
    /// Template Ollama api call.
    /// </summary>
    /// <param name="ClientFactory">Ollama setup factory.</param>
    public OllamaResponse (
        IOllamaClientFactory ClientFactory,
        IOptions<AppConfiguration> config)
    {
        var client = ClientFactory.FindClient(OllamaServiceType.Ollama);

        var httpClient = new HttpClient()
        {
            BaseAddress = client.Uri,
            Timeout = config.Value.OllamaOptions.Timeout,
        };

        _ollamaClient = new OllamaApiClient(httpClient)
        {
            SelectedModel = client.SelectedModel,
        };
    }

    /// <summary>Streams and returns full LLM response.</summary>
    /// <param name="request">Generation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Full generated text.</returns>
    /// <example><![CDATA[string answer = await provider.GenerateResponse(request, ct);]]></example>
    public async Task<string> GenerateResponse (
        GenerateRequest request,
        CancellationToken ct)
    {
        request.Options = new()
        {
            Temperature = 0.2f,
            RepeatPenalty = 1.02f,
        };

        var sb = new StringBuilder();

        var panelText = new Markup(" Waiting for response... ", Styles.Yellow).LeftJustified();

        var headerText = " Generating... ";

        var panel = new Panel(panelText)
            .Header(headerText)
            .BorderColor(Color.Green)
            .RoundedBorder()
            .BorderStyle(Styles.GreenBlink)
            .Expand()
            .Padding(5, 1, 1, 5);

        try
        {
            await AnsiConsole.Live(panel).StartAsync(async ctx =>
            {
                ctx.UpdateTarget(panel);
                ctx.Refresh();

                try
                {
                    await foreach(var stream in _ollamaClient.GenerateAsync(request, ct))
                    {
                        if(stream is null)
                            throw new InvalidOperationException("Stream returned null response.");

                        if(stream?.Response is not { } response) continue;

                        sb.Append(response.AsSpan());

                        panel.BorderStyle = null;

                        panelText = new Markup(sb.ToString().EscapeMarkup(), Styles.Yellow);

                        headerText =
                        " Streaming Response ";

                        panel.Header(headerText);

                        ctx.UpdateTarget(panelText);

                        ctx.Refresh();
                    }
                }
                catch(Exception ex)
                {
                    AnsiConsole.WriteException(ex);
                }
            });

            return sb.ToString();
        }
        catch(Exception ex)
        {
            AnsiConsole.WriteException(ex);
            throw;
        }
        finally
        {
            sb.Clear();
        }
    }
}
