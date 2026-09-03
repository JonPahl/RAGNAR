using Ragnar.Questions.Interface;

namespace Ragnar;

public static class RagPipelineHostBuilder
{
    public static IHostBuilder CreateDefaultBuilder(string[] args) =>
        Host
        .CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(config =>
        {
            const string X = "Ragnar";
            // var x = "AspireObit";
            // const string X = "Shepherd_Api";

            config
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{X}.json", optional: true, reloadOnChange: true);
        })
        .ConfigureServices((context, services) =>
        {
            services
                .AddSingleton<IQuestionBuilder, QuestionBuilder>();
            services
            .AddSingleton<ConfigToQuestionMapper>()
            .AddScoped<IQuestionEmbedding, QuestionEmbedding>()
            .AddSingleton<IOllamaClientFactory, OllamaClientFactory>();

            services
            .AddSingleton<IKnowledgeBaseInitialize, KnowledgeBaseInitialization>()
            .AddSingleton<IApplicationHeader, ApplicationHeader>()
            .AddSingleton<ISummaryService, SummaryService>()
            .AddScoped<IResponseWriter, ResponseWriter>()
            .AddScoped<IOutputFormatter, ResponseMarkdownFormatter>();
            services.AddSingleton<IRagOrchestrator, RagOrchestrator>();

            services.AddScoped<IFileValidator, FileValidator>();
            services.AddKeyedSingleton<IPromptProvider, PromptTemplateProvider>("Common");
            services.AddKeyedSingleton<IPromptProvider, SummarizePromptProvider>("Summary");

            services.AddSingleton<IQuestionCatalogLoader, DefaultQuestionCatalogLoader>();
            services.AddSingleton<IQuestionCombineBuilder, QuestionCombineBuilder>();

            // Infrastructure
            services.AddSingleton<IQdrantClient>(sp =>
            {
                var option = sp.GetRequiredService<IOptions<RagnarConfig>>().Value.EmbeddingOptions;

                return new QdrantClient(option.Host, option.Port, https: false);
            });

            services
            .AddSingleton<IOllamaAIClientBuilder, OllamaAIClientBuilder>();
            services.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();

            services.AddSingleton<IOllamaGenerationService, OllamaChatResponse>();
            services.AddSingleton<IProgressReporter, ProgressReporter>();

            // Pipeline Stages (Order preserved by registration)
            SetupPrimaryPipeline(services);

            services
            .AddKeyedScoped<IPipelineStage, QuestionOneStage>("Questions");

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
        services
        .AddKeyedScoped<IPipelineStage, BrandingStage>("Main");

        services
        .AddKeyedScoped<IPipelineStage, KnowledgeBasePreparationStage>("Main");
        services
        .AddKeyedScoped<IPipelineStage, QuestionProcessingStage>("Main");
        services
        .AddKeyedScoped<IPipelineStage, SummarizationStage>("Main");
    }
}
