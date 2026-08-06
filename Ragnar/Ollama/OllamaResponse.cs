namespace Ragnar.Ollama;

/// <summary>
/// Configures and caches OllamaOptions clients per model.
/// </summary>
/// <example><![CDATA[var provider = new OllamaResponse(opts);]]></example>
public class OllamaResponse : IOllamaResponse
{
    private readonly OllamaApiClient OllamaClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaResponse"/> class.
    /// Template Ollama api call.
    /// </summary>
    /// <param name="ClientFactory">Ollama setup factory.</param>
    /// <param name="Config"></param>
    public OllamaResponse(
        IOllamaClientFactory ClientFactory,
        IOptions<AppConfiguration> Config)
    {
        var Client = ClientFactory.FindClient(OllamaServiceType.Ollama);

        var HttpClient = new HttpClient()
        {
            BaseAddress = Client.Uri,
            Timeout = Config.Value.OllamaOptions.Timeout,
        };

        OllamaClient = new OllamaApiClient(HttpClient)
        {
            SelectedModel = Client.SelectedModel,
        };
    }

    /// <summary>Streams and returns full LLM response.</summary>
    /// <param name="Request">Generation request.</param>
    /// <param name="Ct">Cancellation token.</param>
    /// <returns>Full generated text.</returns>
    /// <example><![CDATA[string answer = await provider.GenerateResponse(request, ct);]]></example>
    public async Task<string> GenerateResponse(
        GenerateRequest Request,
        CancellationToken Ct)
    {
        Request.Options = new()
        {
            Temperature = 0.2f,
            RepeatPenalty = 1.02f,
        };

        var Sb = new StringBuilder();

        var PanelText = new Markup(" Waiting for response... ", Styles.Yellow).LeftJustified();

        var HeaderText = " Generating... ";

        var Panel = new Panel(PanelText)
            .Header(HeaderText)
            .BorderColor(Color.Green)
            .RoundedBorder()
            .BorderStyle(Styles.GreenBlink)
            .Expand()
            .Padding(5, 1, 1, 5);

        try
        {
            await AnsiConsole.Live(Panel).StartAsync(async Ctx =>
            {
                Ctx.UpdateTarget(Panel);
                Ctx.Refresh();

                try
                {
                    await foreach(var Stream in OllamaClient.GenerateAsync(Request, Ct))
                    {
                        if(Stream is null)
                            throw new InvalidOperationException("Stream returned null response.");

                        if(Stream?.Response is not { } Response) continue;

                        Sb.Append(Response.AsSpan());

                        Panel.BorderStyle = null;

                        PanelText = new Markup(Sb.ToString().EscapeMarkup(), Styles.Yellow);

                        HeaderText =
                        " Streaming Response ";

                        Panel.Header(HeaderText);

                        Ctx.UpdateTarget(PanelText);

                        Ctx.Refresh();
                    }
                }
                catch(Exception Ex)
                {
                    AnsiConsole.WriteException(Ex);
                }
            });

            return Sb.ToString();
        }
        catch(Exception Ex)
        {
            AnsiConsole.WriteException(Ex);
            throw;
        }
        finally
        {
            Sb.Clear();
        }
    }
}
