using System.Text;

namespace Ragnar.Ollama;
/// <summary>
/// Configures and caches OllamaOptions clients per model.
/// </summary>
/// <example><![CDATA[var provider = new OllamaResponse(opts);]]></example>
public class OllamaResponse : IOllamaResponse
{
    private readonly OllamaApiClient ollamaClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaResponse"/> class.
    /// Template Ollama api call.
    /// </summary>
    /// <param name="ClientFactory">Ollama setup factory.</param>
    public OllamaResponse(IOllamaClientProvider ClientFactory)
    {
        var client = ClientFactory.FindClient(OllamaType.Ollama);

        var httpClient = new HttpClient()
        {
            BaseAddress = client.Uri,
            Timeout = TimeSpan.FromMinutes(20),
        };
        ollamaClient = new OllamaApiClient(httpClient)
        { SelectedModel = client.SelectedModel, };
    }

    /// <summary>Streams and collects full LLM response into a string.</summary>
    /// <param name="request">Generation request with prompt/options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Full generated text.</returns>
    /// <example><![CDATA[string answer = await provider.GenerateResponse(request, ct);]]></example>
    public async Task<string> GenerateResponse(
        GenerateRequest request,
        CancellationToken ct)
    {
        request.Options = new()
        {
            Temperature = 0.2f,
            RepeatPenalty = 1.02f,
        };

        var sb = new StringBuilder();

        var panelText = new Markup(string.Empty, Styles.Yellow).LeftJustified();

        var headerText = " Generating... ";

        var panel = new Panel(panelText)
            .Header(headerText)
            .BorderColor(Color.Green)
            .RoundedBorder()
            .BorderStyle(Styles.GreenBlink)
            .Expand()
            .Padding(1, 1, 1, 1);

        try
        {
            await AnsiConsole.Live(panel).StartAsync(async ctx =>
            {
                ctx.UpdateTarget(panel);
                ctx.Refresh();

                await foreach (var stream in ollamaClient.GenerateAsync(request, ct))
                {
                    if (stream is null)
                        throw new InvalidOperationException("Stream returned null response.");

                    sb.Append(stream.Response.AsSpan());

                    panel.BorderStyle = null;

                    panelText = new Markup(sb.ToString().EscapeMarkup(), Styles.Yellow);

                    headerText = " Streaming Response ";

                    ctx.UpdateTarget(panelText);
                    ctx.Refresh();
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
