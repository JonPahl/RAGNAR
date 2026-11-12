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

            cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
           .AddJsonFile($"appsettings.{X}.json", optional: true, reloadOnChange: true);
        })
            .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration))
            .ConfigureServices((ctx, services) =>
            {
                services.AddSingleton<Microsoft.Extensions.Logging.ILogger>(sp =>
                {
                    var log = sp.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()
                    .CreateLogger("Ragnar");
                    return log;
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
        s.AddSingleton<IFileInclusionFilter, FileValidator>();

        s.AddSingleton<ISyntaxTreeChunker, SyntaxTreeChunker>();
        s.AddSingleton<IVectorStoreBuilder, VectorStoreBuilder>();
    }

    private static void AddInfrastructure(this IServiceCollection s)
    {
        s.AddSingleton<IQdrantClient>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<RagnarConfig>>().Value.EmbeddingOptions;
            return new QdrantClient(opt.Host.Host, opt.Host.Port, https: false);
        });

        s.AddSingleton<IOllamaChatClientBuilder, OllamaChatClientBuilder>();

        s.AddSingleton<IOllamaGenerationService, OllamaChatService>();
        s.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();
        s.AddHttpClients();
    }

    private static void AddEmbeddingPipeline(this IServiceCollection s)
    {
        s.AddSingleton<IFileFingerprintStore, JsonFileFingerprintStore>();
        s.AddSingleton(sp =>
        {
            var log = sp.GetRequiredService<ILogger>();

            return new JsonFileFingerprintStore(log, sp.GetRequiredService<IOptions<RagnarConfig>>());
        });

        s.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RagnarConfig>>().Value;

            return new EmbeddingContext()
            {
                SourceDirectory = options.ApplicationOptions.SourceDirectory
            };
        });

        s.AddSingleton<IPipeline<EmbeddingContext>>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger>();

            var options = sp.GetRequiredService<IOptions<RagnarConfig>>();

            var header = sp.GetRequiredService<IApplicationHeader>();

            var qdrantClient = sp.GetRequiredService<IQdrantClient>();

            var vectorSetup = sp.GetRequiredService<IVectorSetup>();

            var fileLoader = sp.GetService<IFileDiscoveryService>();
            var outputWriter = sp.GetService<IOutputWriter>();
            var repository = sp.GetService<IVectorStoreWriter>();
            var parseFactory = sp.GetService<IFileParseFactory>();

            var fingerprintStore = sp.GetRequiredService<IFileFingerprintStore>();

            var vectorWriter = sp.GetRequiredService<IVectorStoreWriter>();

            var runner = new PipelineRunner<EmbeddingContext>(logger);

            runner
            .UseMiddleware(new ExceptionHandlingMiddleware(logger).Invoke)
            .UseMiddleware(new StageTimingMiddleware(logger).Invoke)
            .UseMiddleware(new RetryMiddleware(logger).Invoke)
            .UseMiddleware(new ProgressMiddleware(outputWriter).Invoke);

            runner.AddStage(new BrandingStage(logger, header))
                .AddStage(new VectorStoreInitStage(vectorSetup, outputWriter))
                .AddStage(new DiscoveryStage(options, fileLoader, logger, outputWriter))
                .AddStage(new ParsingStage(parseFactory, logger))
                .AddStage(new DeltaEmbedStage(logger, fingerprintStore, vectorWriter, options))
                .AddStage(new UpsertStage(repository, logger, options, outputWriter))
                .AddStage(new StoredFileListStage(options, qdrantClient, logger));

            return runner;

        });
    }

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

        //s.AddSingleton(s.AddSingleton<IPipeline<QuestionPipelineContext>, QuestionPipelineRunner>(sp =>

        s.AddSingleton<IPipeline<QuestionPipelineContext>>(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger>();
            var writer = sp.GetRequiredService<IOutputWriter>();

            var runner = new PipelineRunner<QuestionPipelineContext>(logger);

            runner.UseMiddleware(new ExceptionHandlingMiddleware(logger).Invoke)
            .UseMiddleware(new StageTimingMiddleware(logger).Invoke)
            .UseMiddleware(new RetryMiddleware(logger).Invoke)
            .UseMiddleware(new ProgressMiddleware(writer).Invoke);

            runner.AddStage(new QuestionLoadStage(
               sp.GetRequiredService<IQuestionSource>(),
               sp.GetRequiredService<IQuestionSourceBuilder>(),
               sp.GetRequiredService<ILogger>()))
            .AddStage(new QuestionTableStage(sp.GetRequiredService<IOutputWriter>()))
            .AddStage(new QuestionAnswerStage(
                sp.GetRequiredService<IRagOrchestrator>(),
                sp.GetRequiredService<IOptions<RagnarConfig>>(),
                sp.GetRequiredService<IOutputWriter>(),
                sp.GetRequiredService<IContextRetriever>(),
                sp.GetRequiredService<ILogger>()))
            .AddStage(new SummarizationStage(
                sp.GetRequiredService<IResponseSummarizer>(),
                sp.GetRequiredService<IOutputWriter>()
                ));

            return runner;
        });
    }
}
