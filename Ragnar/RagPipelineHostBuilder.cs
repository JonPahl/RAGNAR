namespace Ragnar;

public static class RagPipelineHostBuilder
{
    public static IHostBuilder CreateDefaultBuilder(string[] args) =>
        Host
        .CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(config =>
        {
            // TODO How do I make changing what appsettings file to use easier.

            const string X = "Ragnar";
            // var x = "AspireObit";
            // const string X = "Shepherd_Api";
            //const string X = "ImageDescriptionWin";

            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{X}.json", optional: true, reloadOnChange: true);
        })
        .ConfigureServices((context, services) =>
        {
            services.AddValidatorsFromAssembly(typeof(Program).Assembly);

            services
                .AddSingleton<IQuestionBuilder, QuestionBuilder>();
            services
            .AddSingleton<ConfigToQuestionMapper>()
            .AddSingleton<IContextRetriever, ContextRetriever>()

            .AddSingleton<IChunkBySyntaxTree, ChunkBySyntaxTree>()
            .AddSingleton<IOllamaClientFactory, OllamaClientFactory>();

            services
            //.AddSingleton<IKnowledgeBaseSeeder, KnowledgeBaseInitialization>()
            .AddSingleton<IApplicationHeader, ApplicationHeader>()
            .AddSingleton<IResponseSummarizer, SummaryService>()
            .AddScoped<IResponseWriter, ResponseWriter>()
            .AddSingleton<IVectorStoreBuilder, VectorStoreBuilder>()
            .AddSingleton<IFileDiscoveryService, FileDiscoveryService>()

            .AddScoped<IOutputFormatter, ResponseMarkdownFormatter>();
            services.AddSingleton<IRagOrchestrator, RagOrchestrator>();

            services
            .AddSingleton<IQuestionCatalogLoader, DefaultQuestionCatalogLoader>()
            .AddSingleton<IConfigurationLoader, FileConfigLoader>()
            .AddSingleton<IRecordParser<QuestionRecord>, CsvRecordParser>()
            .AddSingleton<IQuestionSource, CsvQuestionSource>();

            services.AddScoped<IFileValidator, FileValidator>();
            services.AddKeyedSingleton<IChatPromptProvider, PromptTemplateProvider>("Common");
            services.AddKeyedSingleton<IChatPromptProvider, SummarizePromptProvider>("Summary");

            services.AddSingleton<IQuestionCatalogLoader, DefaultQuestionCatalogLoader>();
            services.AddSingleton<IQuestionSourceBuilder, QuestionCombineBuilder>();

            // Infrastructure
            services.AddSingleton<IQdrantClient>(sp =>
            {
                var option = sp.GetRequiredService<IOptions<RagnarConfig>>().Value.EmbeddingOptions;

                return new QdrantClient(option.Host, option.Port, https: false);
            });

            services
            .AddSingleton<IOllamaAIClientBuilder, OllamaAIClientBuilder>();
            services.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();

            services.AddSingleton<IOllamaGenerationService, OllamaChatService>();

            // Pipeline Stages (Order preserved by registration)
            SetupPrimaryPipeline(services);

            services.AddSingleton<QuestionFactoryDelegate>(_ =>
             (text, key, category, isActive) => new Core.Model.Question(isActive, text, key, category));
            services.AddSingleton<IQuestionBuilder, QuestionBuilder>();
            services.AddSingleton<IOutputFormatter, ResponseMarkdownFormatter>();
            services.AddSingleton<IPathResolver, PathResolver>();
            services.AddSingleton<IWriter, FileWriter>();
            services.AddSingleton<ConfigToQuestionMapper>();
            services.AddSingleton<IQdrantClient, QdrantClient>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<RagnarConfig>>().Value.EmbeddingOptions;

                return new QdrantClient(options.Host, options.Port, https: false);
            })
            .AddSingleton<IOllamaClientFactory, OllamaClientFactory>();
            services.AddSingleton<IOutputWriter, AnsiConsoleOutputWriter>();
            services.AddSingleton<IClock, SystemClock>();

            // Configuration & Plugins

            services.RegisterEmbeddingServices();
            services.RegisterOptions(context);
            services.AddHttpClients();
            services.LoadQuestionPlugins();
            services.EmbeddingSetup();
            services.AddHostedService<RagPipelineRunner>();
        }).UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

    private static void SetupPrimaryPipeline(IServiceCollection services)
    {

        services.AddSingleton(sp => new QuestionPipelineContext()
        );

        services.AddSingleton(sp => new EmbeddingContext
        {
            SourceDirectory = sp.GetRequiredService<IOptions<RagnarConfig>>()
                .Value.ApplicationOptions.SourceDirectory
        });

        // Register each stage (order matters)
        services.AddSingleton<IPipelineRunner<EmbeddingContext>, PipelineRunner>(provider =>
        {
            var writer = provider.GetRequiredService<IOutputWriter>();

            var logger = provider.GetRequiredService<Serilog.ILogger>();

            var options = provider
            .GetRequiredService<IOptions<RagnarConfig>>();

            var fileProvider = provider.GetRequiredService<IFileDiscoveryService>();

            var parseFactory = provider.GetRequiredService<IFileParseFactory>();

            var ragOrchestrator = provider.GetRequiredService<IRagOrchestrator>();

            var questionEmbedding = provider.GetRequiredService<IContextRetriever>();

            var questionBuilder = provider.GetRequiredService<IQuestionSourceBuilder>();

            var header = provider.GetRequiredService<IApplicationHeader>();

            var summaryService = provider.GetRequiredService<IResponseSummarizer>();

            var vectorStoreWriter = provider.GetRequiredService<IVectorStoreWriter>();

            var qdrantClient = provider.GetRequiredService<IQdrantClient>();

            var vectorSetup = provider.GetRequiredService<IVectorSetup>();

            var questionSource = provider.GetRequiredService<IQuestionSource>();

            var runner = new PipelineRunner(logger, writer)
            .AddStage(new BrandingStage(header))
            .AddStage(new KnowledgeBasePreparationStage(vectorSetup, writer))
            .AddStage(new DiscoveryStage(options, fileProvider, logger, writer))
            .AddStage(new ParsingStage(parseFactory, logger))
            .AddStage(new UpsertStage(vectorStoreWriter, logger, options, writer))
            .AddStage(new ShowFileStage(options, qdrantClient, logger));


            return runner;
        });
        services.AddSingleton<IPipelineRunner<QuestionPipelineContext>, QuestionPipelineRunner>(provider =>
        {
            var writer = provider.GetRequiredService<IOutputWriter>();

            var logger = provider.GetRequiredService<Serilog.ILogger>();

            var options = provider
            .GetRequiredService<IOptions<RagnarConfig>>();

            var fileProvider = provider.GetRequiredService<IFileDiscoveryService>();

            var parseFactory = provider.GetRequiredService<IFileParseFactory>();

            var ragOrchestrator = provider.GetRequiredService<IRagOrchestrator>();

            var questionEmbedding = provider.GetRequiredService<IContextRetriever>();

            var questionBuilder = provider.GetRequiredService<IQuestionSourceBuilder>();

            var header = provider.GetRequiredService<IApplicationHeader>();

            var summaryService = provider.GetRequiredService<IResponseSummarizer>();

            var vectorStoreWriter = provider.GetRequiredService<IVectorStoreWriter>();

            var qdrantClient = provider.GetRequiredService<IQdrantClient>();

            var vectorSetup = provider.GetRequiredService<IVectorSetup>();

            var questionSource = provider.GetRequiredService<IQuestionSource>();

            var x = new QuestionPipelineRunner(logger, writer)
            .AddStage(new QuestionLoadStage(questionSource, questionBuilder, writer, logger))
            .AddStage(new QuestionShowStage())
            .AddStage(new SummarizationStage(summaryService, writer));

            return x;
        });


        //    var runner = new PipelineRunner(writer, logger)
        //.AddStage(new DiscoveryStage(fileProvider, logger, writer))
        //.AddStage(new ParsingStage(parseFactory, logger, writer))
        //.AddStage(new EmbeddingStage(embeddingSvc, vectorSvc, logger, writer))
        //.SetRetryPolicy("Parsing files…", maxAttempts: 5,
        //    attempt => TimeSpan.FromSeconds(Math.Pow(1.5, attempt)))   // gentler backoff
        //.SetRetryPolicy("Embedding…", maxAttempts: 2,
        //    attempt => TimeSpan.FromMilliseconds(500));
        //    services.AddSingleton<IPipelineRunner>(runner);
        //services.AddSingleton<IPipelineStage<EmbeddingContext>, BrandingStage>();
        //services.AddSingleton<IPipelineStage<EmbeddingContext>, KnowledgeBasePreparationStage>();

        //services.AddSingleton<IPipelineStage<EmbeddingContext>, DiscoveryStage>();
        //services.AddSingleton<IPipelineStage<EmbeddingContext>, ParsingStage>();
        //services.AddSingleton<IPipelineStage<EmbeddingContext>, UpsertStage>();

        //services.AddSingleton<IPipelineStage<EmbeddingContext>, ShowFileStage>();

        // TODO Add Stage that loads questions and passes them in their own context to QuestionProcessingStage.

        //services.AddSingleton<IPipelineStage<EmbeddingContext>, QuestionShowStage>();
        //services.AddSingleton<IPipelineStage<EmbeddingContext>, SummarizationStage>();
    }
}
