namespace Ragnar;

/// <summary>
/// Builds the default .NET 10 generic host pre-configured for the Ragnar RAG pipeline.
/// </summary>
public static partial class RagPipelineHostBuilder
{
    /// <summary>
    /// Builds the default .NET 10 generic host configured for the Ragnar RAG pipeline.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the host.</param>
    /// <returns>A configured <see cref="IHostBuilder"/> ready for <c>Build()</c>.</returns>
    /// <example>
    /// <![CDATA[var host = RagPipelineHostBuilder.CreateDefaultBuilder(args).Build();]]>
    /// </example>
    public static IHostBuilder CreateDefaultBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(cfg =>
        {
            // TODO How do I add an interactive mode allowing the user to ask custom questions?

            // TODO How do I make changing what appsettings file to use easier.

            //const string X = "Pipeline";
            const string X = "Ragnar";
            //const string X = "Bank";
            //const string X = "Shepherd_Api";
            //const string X = "ObitLoader";
            //const string X = "SolutionBuilder";

            cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
           .AddJsonFile($"appsettings.{X}.json", optional: true, reloadOnChange: true);
        })
            .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration))
            .ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<ILogger>(sp =>
                {
                    return sp.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Ragnar");
                });

                services.AddValidatorsFromAssembly(typeof(Program).Assembly);
                services.RegisterOptions(ctx);
                services.AddHttpClients();
                services.LoadQuestionPlugins();
                services.CodeParser();
                services.EmbeddingSetup();

                services.AddCoreServices();
                services.AddInfrastructure();
                services.AddEmbeddingPipeline();
                services.AddQuestionPipeline();
                services.AddHostedService<RagPipelineRunner>();
            });
    }

    private static void AddCoreServices(this IServiceCollection s)
    {
        s.AddKeyedSingleton<IChatPromptProvider, PromptTemplateProvider>("Common");
        s.AddKeyedSingleton<IChatPromptProvider, SummarizePromptProvider>("Summary");

        s.AddSingleton<IApplicationHeader, ApplicationHeader>();
        s.AddSingleton<IResponseSummarizer, SummaryService>();
        s.AddSingleton<IResponseWriter, ResponseWriter>();
        s.AddSingleton<IRagPromptBuilder, RagPromptBuilder>();
        s.AddSingleton<IOutputWriter, AnsiConsoleOutputWriter>();
        s.AddSingleton<IClock, SystemClock>();
        s.AddSingleton<IWriter, FileWriter>();
        s.AddSingleton<IPathResolver, PathResolver>();
        s.AddSingleton<IOutputFormatter, ResponseMarkdownFormatter>();
        s.AddSingleton<IFileDiscoveryService, FileDiscoveryService>();
        s.AddSingleton<IFileParseFactory, CodeParserFactory>();
        s.AddSingleton<ISyntaxTreeChunker, SyntaxTreeChunker>();
        s.AddSingleton<IVectorStoreBuilder, VectorStoreBuilder>();
    }

    /// <summary>Registers infrastructure services like clients and factories into DI.</summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The updated service collection for chaining.</returns>
    /// <example><![CDATA[builder.Services.AddInfrastructure();]]></example>
    private static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IQdrantClient>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<RagnarConfig>>().Value.EmbeddingOptions;
            return new QdrantClient(opt.Host.Host, opt.Host.Port, https: false);
        });

        services.AddSingleton<IOllamaChatClientBuilder, OllamaChatClientBuilder>();

        services.AddSingleton<IOllamaGenerationService, OllamaChatService>();
        services.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();
        services.AddHttpClients();
    }

    /// <summary>Registers embedding pipeline services into the DI container.</summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The updated service collection for chaining.</returns>
    /// <example><![CDATA[builder.Services.AddEmbeddingPipeline();]]></example>
    private static void AddEmbeddingPipeline(this IServiceCollection services)
    {
        services.AddSingleton<IFileFingerprintStore, JsonFileFingerprintStore>();
        services.AddSingleton(sp =>
        {
            var log = sp.GetRequiredService<ILogger>();

            return new JsonFileFingerprintStore(log, sp.GetRequiredService<IOptions<RagnarConfig>>());
        });

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RagnarConfig>>().Value;

            return new EmbeddingContext()
            {
                SourceDirectory = options.ApplicationOptions.SourceDirectory
            };
        });
    }

    /// <summary>Registers question pipeline services into the DI container.</summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The updated service collection for chaining.</returns>
    /// <example><![CDATA[builder.Services.AddQuestionPipeline();]]></example>
    private static void AddQuestionPipeline(this IServiceCollection s)
    {
        s.AddSingleton<IContextRetriever, ContextRetriever>();
        s.AddSingleton<IQuestionSource, CsvQuestionSource>();
        s.AddSingleton<IQuestionSourceBuilder, QuestionAggregator>();
        s.AddSingleton<IQuestionCatalogLoader, DefaultQuestionCatalogLoader>();
        s.AddSingleton<IConfigurationLoader, FileConfigLoader>();
        s.AddSingleton<IRecordParser<QuestionRecord>, CsvRecordParser>();
        s.AddSingleton<IQuestionBuilder, QuestionBuilder>();
        s.AddSingleton<IRagOrchestrator, RagOrchestrator>();
        s.AddSingleton(new QuestionPipelineContext());
    }
}
