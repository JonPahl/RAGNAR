using ChatRole = OllamaSharp.Models.Chat.ChatRole;

namespace Ragnar.Ollama;

/// <summary>Streams Ollama chat responses with live console rendering.</summary>
/// <remarks>Applies token limits, temperature, and real-time panel updates.</remarks>
/// <example><![CDATA[var text = await response.GenerateResponse(req, ct);]]></example>
public class OllamaChatResponse(
    Serilog.ILogger logger,
    IOllamaClientFactory clientFactory)
    : IOllamaGenerationService
{
    private readonly OllamaApiClient _ollamaClient = clientFactory.FindClient(OllamaServiceType.Ollama);

    /// <summary>Generates a chat response using configured Ollama options.</summary>
    /// <param name="request">The generation request containing prompt and system info.</param>
    /// <param name="cancellationToken">Cancellation token to abort the operation.</param>
    /// <returns>The complete generated text response.</returns>
    public async Task<string> GenerateResponse(GenerateRequest request, CancellationToken cancellationToken)
    {
        RequestOptions requestOptions = new()
        {
            NumPredict = 8192,
            NumCtx = 16384,
            NumThread = 8,
            Temperature = 0.2f,
            RepeatPenalty = 1.02f,
        };

        request.Options = requestOptions;

        Chat client = new(_ollamaClient, request.System);
        client.Messages.Add(new Message(ChatRole.System, request.System));

        StringBuilder completeText = new();
        StringBuilder thinkingText = new();

        var headerText = new PanelHeader(" Generating... ");

        var panelText = new Markup(string.Empty, Styles.Yellow).LeftJustified();

        var panel = new Panel(panelText)
            .Header(headerText)
            .BorderColor(Color.Green)
            .RoundedBorder()
            .BorderStyle(Styles.GreenBlink)
            .Expand()
            .Padding(1, 1, 1, 1);

        // 3. Start Live Render Loop
        await AnsiConsole.Live(panel)
            .StartAsync(async ctx =>
        {
            ctx.Refresh();
            try
            {
                client.Think = ThinkValue.Medium;
                client.OnThink += (sender, token) =>
                {
                    headerText = new PanelHeader("Thinking");
                    var clean = Markup.Escape(token);
                    thinkingText.Append(clean);

                    panelText = new Markup(thinkingText.ToString(), Styles.Red);

                    ctx.UpdateTarget(panel);
                    ctx.UpdateTarget(panelText);
                    ctx.Refresh();
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger?.Error(ex, "Thinking stream failed: {Msg}", ex.Message);
                completeText.Append($"\n\n[[ERROR]] Thinking stream interrupted: {ex.Message}\n");
                // Optionally re-throw or mark the response as degraded
            }


            // Await foreach loop running alongside the spinner
            await foreach (var token in client.SendAsAsync(ChatRole.User, request.Prompt, cancellationToken: cancellationToken))
            {
                if (token is null)
                {
                    throw new ArgumentException("null response is null");
                }

                var clean = Markup.Escape(token);
                completeText.Append(clean);

                panelText = new Markup(completeText.ToString(), Styles.Yellow);

                ctx.UpdateTarget(panel);
                ctx.UpdateTarget(panelText);
                ctx.Refresh();
            }
        });

        return completeText.ToString();
    }
}
