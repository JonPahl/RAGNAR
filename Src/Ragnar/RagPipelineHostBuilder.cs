namespace Ragnar;

/// <summary>
/// Builds the default .NET 10 generic host pre-configured for the Ragnar RAG pipeline.
/// </summary>
public static partial class RagPipelineHostBuilder
{

    private static RagnarConfig ragnarConfig;

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

        //const string X = "Pipeline";
        //const string X = "Ragnar";
        //const string X = "Bank";
        //const string X = "Shepherd_Api";
        //const string X = "ObitLoader";
        //const string X = "SolutionBuilder";

        var hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(cfg =>
        {
            // TODO How do I add an interactive mode allowing the user to ask custom questions?

            // TODO How do I make changing what appsettings file to load easier.
            cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
        });

        hostBuilder.Properties.Add("File", "Ragnar");
        var X = hostBuilder.Properties["File"];

        hostBuilder.ConfigureAppConfiguration(x => x.AddJsonFile($"appsettings.{X}.json", optional: true, reloadOnChange: true));

        hostBuilder
            .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration))
            .ConfigureServices((ctx, services) =>
            {
                ctx.Configuration.GetSection("RagnarConfig").Bind(ragnarConfig);

                services.AddSingleton(sp =>
                {
                    var log = sp.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Ragnar");
                    return log;
                });


                services.AddValidatorsFromAssembly(typeof(Program).Assembly);
                services.RegisterOptions(ctx);
                services.AddHttpClients();
                services.LoadQuestionPlugins();

                #region xxx
                services.AddSingleton<IContextRetriever, ContextRetriever>();
                services.AddSingleton<IRagOrchestrator, RagOrchestrator>();
                services.AddSingleton<IRagSession, RagSession>();

                services.AddSingleton<IReportExporter, ReportExporter>();

                #endregion

                services.CodeParser();
                services.EmbeddingSetup();

                services.AddCoreServices();
                services.AddInfrastructure();
                services.AddEmbeddingPipeline();
                services.AddQuestionPipeline();
                services.AddHostedService<RagPipelineRunner>();
            });

        return hostBuilder;
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

        var connectionString = "Data Source=questions.db";

        //builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=questions.db";

        // 2. Register DbContext with the DI container
        s.AddDbContext<QuestionDbContext>(options =>
            options.UseSqlite(connectionString));
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
    }

    private static void AddQuestionPipeline(this IServiceCollection s)
    {
        s.AddSingleton<IContextRetriever, ContextRetriever>();
        s.AddSingleton<ISqliteDbService, SqliteDbService>();
        s.AddSingleton<IQuestionSource, CsvQuestionSource>();
        s.AddSingleton<IQuestionSource, SqLiteSource>();

        s.AddSingleton<IQuestionSourceBuilder, QuestionAggregator>();
        s.AddSingleton<IQuestionCatalogLoader, DefaultQuestionCatalogLoader>();
        s.AddSingleton<IConfigurationLoader, FileConfigLoader>();
        s.AddSingleton<IRecordParser<QuestionRecord>, CsvRecordParser>();

        s.AddSingleton<ISqliteDbService, SqliteDbService>(sp =>
        {
            var options = new DbContextOptions<QuestionDbContext>();
            var x = new QuestionDbContext(options);
            var writer = sp.GetRequiredService<IOutputWriter>();
            return new SqliteDbService(x, writer);
        });

        s.AddSingleton<IQuestionBuilder, QuestionBuilder>();
        s.AddSingleton<IRagOrchestrator, RagOrchestrator>();
        s.AddSingleton(new QuestionPipelineContext());
    }
}
