namespace Ragnar.Ollama;

/// <summary>Streams Ollama chat responses with live console rendering.</summary>
/// <remarks>Applies token limits, temperature, and real-time panel updates.</remarks>
/// <example><![CDATA[var text = await response.GenerateResponse(req, ct);]]></example>
public class OllamaChatService(
    ILogger logger,
    IOptions<RagnarConfig> options,
    IOllamaClientFactory clientFactory)
    : IOllamaGenerationService
{
    /// <summary>Holds the pre-resolved Ollama API client for chat generation calls.</summary>
    /// <value>An OllamaApiClient configured with the LLM service endpoint.</value>
    /// <example><![CDATA[// Resolved once at construction via the client factory.]]></example>
    private readonly IOllamaApiClient _ollamaClient = clientFactory.GetClient(OllamaServiceType.Ollama);

    /// <summary>Generates a chat response using configured Ollama options.</summary>
    /// <param name="request">The generation request containing prompt and system info.</param>
    /// <param name="cancellationToken">Cancellation token to abort the operation.</param>
    /// <returns>The complete generated text response.</returns>
    /// <example><![CDATA[string txt = await resp.GenerateResponse(req, ct);]]></example>

    public async Task<string> GenerateResponse(GenerateRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Guard.Against.Null(request);
        request.Options = BuildOptions(options.Value);

        var model = options.Value.OllamaOptions.LlmModel;
        List<Message> messages = [new Message(ChatRole.System, request.System)];

        Chat client = new(_ollamaClient, model)
        {
            Messages = messages,
        };

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
        await AnsiConsole.Live(panel).StartAsync(async ctx =>
        {
            ctx.Refresh();
            try
            {
                client.Think = ThinkValue.Low;

                client.OnThink += (sender, token) =>
                {

                    AnsiConsole.Progress()
                    .IncludeVerticalPadding()
                    .Columns(
                        new TaskDescriptionColumn(),
                        new ProgressBarColumn()
                        {
                            CompletedStyle = new Style(Color.Green),
                            FinishedStyle = new Style(Color.Lime),
                            RemainingStyle = new Style(Color.Grey)
                        },
                        new PercentageColumn())
                    .Start(ctx =>
                    {
                        var task = ctx.AddTask("Building project").IsIndeterminate();

                        while (!ctx.IsFinished)
                        {
                            task.Increment(1);
                            Thread.Sleep(40);
                        }
                        ctx.RemoveTask(task);
                    });

                    //headerText = new PanelHeader("Thinking");
                    //var clean = Markup.Escape(token);
                    //thinkingText.Append(clean);

                    //panelText = new Markup(thinkingText.ToString(), Styles.Red);

                    //ctx.UpdateTarget(panel);
                    //ctx.UpdateTarget(panelText);
                    //ctx.Refresh();
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger?.LogCritical(ex, "Thinking stream failed: {Msg}", ex.Message);
                completeText.Append($"\n\n[ERROR] Thinking stream interrupted: {ex.Message}\n");
            }

            try
            {
                await foreach (var token in client.SendAsAsync(ChatRole.User, request.Prompt, cancellationToken: cancellationToken).ConfigureAwait(false))
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
            }
            catch (TaskCanceledException ex)
            {
                logger.LogCritical(ex, ex.Message);
                var cancelMsg = $"\n\n[INFO] Generation canceled: {ex.Message}\n";
                completeText.Insert(0, cancelMsg);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, ex.Message);
                throw;
            }
        }).ConfigureAwait(false);

        return completeText.ToString();
    }

    private RequestOptions? BuildOptions(RagnarConfig value)
    {
        var generation = value.GenerationOptions;

        return new RequestOptions
        {
            NumPredict = generation.MaxTokens,
            NumCtx = generation.ContextWindow,
            NumThread = generation.ThreadCount,
            Temperature = generation.Temperature,
            RepeatPenalty = generation.RepeatPenalty,
        };
    }
}
