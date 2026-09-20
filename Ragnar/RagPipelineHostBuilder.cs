using Ragnar.Embedding.Chunker;

namespace Ragnar;

public static class RagPipelineHostBuilder
{
    public static IHostBuilder CreateDefaultBuilder(string[] args) =>
        Host
        .CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(config =>
        {
            // TODO How do I make changing what appsettings file to use easier.

            //const string X = "Ragnar";
            // var x = "AspireObit";
            // const string X = "Shepherd_Api";
            const string X = "ImageDescriptionWin";

            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{X}.json", optional: true, reloadOnChange: true);
        })
        .ConfigureServices((context, services) =>
        {
            services
                .AddSingleton<IQuestionBuilder, QuestionBuilder>();
            services
            .AddSingleton<ConfigToQuestionMapper>()
            .AddScoped<IQuestionEmbedding, QuestionEmbedding>()

            .AddSingleton<IChunkBySyntaxTree, ChunkBySyntaxTree>()
            .AddSingleton<IOllamaClientFactory, OllamaClientFactory>();

            services
            //.AddSingleton<IKnowledgeBaseSeeder, KnowledgeBaseInitialization>()
            .AddSingleton<IApplicationHeader, ApplicationHeader>()
            .AddSingleton<ISummaryService, SummaryService>()
            .AddScoped<IResponseWriter, ResponseWriter>()
            .AddSingleton<IVectorStoreBuilder, VectorStoreBuilder>()
            .AddSingleton<IFileDiscoveryService, FileDiscoveryService>()

            .AddScoped<IOutputFormatter, ResponseMarkdownFormatter>();
            services.AddSingleton<IRagOrchestrator, RagOrchestrator>();

            services
            .AddSingleton<IQuestionCatalogLoader, DefaultQuestionCatalogLoader>()
            .AddSingleton<IConfigurationLoader, FileConfigLoader>()
            .AddSingleton<IRecordParser<QuestionRecord>, CsvRecordParser>()
            .AddSingleton<IQuestionProvider, CsvFileQuestionProvider>();

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
            services.AddSingleton<IPipelineRunner, PipelineRunner>();
            services.AddSingleton<IClock, SystemClock>();

            // Configuration & Plugins

            services.RegisterEmbeddingServices();
            services.RegisterOptions(context);
            services.AddHttpClients();
            services.LoadQuestionPlugins();
            services.EmbeddingSetup();
            services.AddHostedService<RagPipelineRunner>();
        })
            .UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

    private static void SetupPrimaryPipeline(IServiceCollection services)
    {
        services.AddSingleton(sp =>
            new EmbeddingContext
            {
                SourceDirectory = sp.GetRequiredService<IOptions<RagnarConfig>>()
        .Value.ApplicationOptions.SourceDirectory
            });

        // Register each stage (order matters)

        services.AddSingleton<IPipelineStage<EmbeddingContext>, BrandingStage>();
        services.AddSingleton<IPipelineStage<EmbeddingContext>, KnowledgeBasePreparationStage>();

        services.AddSingleton<IPipelineStage<EmbeddingContext>, DiscoveryStage>();
        services.AddSingleton<IPipelineStage<EmbeddingContext>, ParsingStage>();
        services.AddSingleton<IPipelineStage<EmbeddingContext>, UpsertStage>();

        services.AddSingleton<IPipelineStage<EmbeddingContext>, ShowFileStage>();

        ////TODO Add Stage that loads questions and passes them in their own context to QuestionProcessingStage.

        services.AddSingleton<IPipelineStage<EmbeddingContext>, QuestionExecutionStage>();

        services.AddSingleton<IPipelineStage<EmbeddingContext>, SummarizationStage>();
    }
}
