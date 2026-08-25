namespace Ragnar;

public static class RagPipelineHostBuilder
{
    public static IHostBuilder CreateDefaultBuilder(string[] args) =>
        Host
        .CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            const string X = "Ragnar";
            // var x = "AspireObit";

            config
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{X}.json", optional: true, reloadOnChange: true);
        })
        .ConfigureServices((context, services) =>
        {
            services
                .AddSingleton<IQuestionFactory, QuestionBuilder>();
            services
            .AddSingleton<ConfigToQuestionMapper>()
            .AddScoped<IQuestionEmbedding, QuestionEmbedding>()
            .AddSingleton<IOllamaClientFactory, OllamaClientFactory>();

            services
            .AddSingleton<IKnowledgeBaseInitialize, KnowledgeBaseInitialization>()
            .AddScoped<IAssemblyInfo, AssemblyInfo>()
            .AddSingleton<IApplicationHeader, ApplicationHeader>()
            .AddSingleton<ISummaryService, SummaryService>()
            .AddScoped<IResponseWriter, ResponseWriter>();

            services.AddSingleton<IRagOrchestrator, RagOrchestrator>();

            services.AddScoped<IFileValidator, FileValidator>();
            services.AddKeyedSingleton<ISystemPromptProvider, SystemPromptProvider>("Common");
            services.AddKeyedSingleton<ISystemPromptProvider, SummarizePromptProvider>("Summary");

            // Infrastructure
            services.AddSingleton<IQdrantClient>(sp =>
            {
                var option = sp.GetRequiredService<IOptions<RagnarConfig>>().Value.EmbeddingOptions;

                return new QdrantClient(option.Host, option.Port, https: false);
            });
            services.AddSingleton<IOllamaClientFactory, OllamaClientFactory>();
            services.AddSingleton<IOllamaResponse, OllamaResponse>();
            services.AddSingleton<IProgressReporter, ProgressReporter>();

            // Pipeline Stages (Order preserved by registration)
            SetupPrimaryPipeline(services);

            services
            .AddKeyedScoped<IPipelineStage, QuestionOneStage>("Questions");
            services.AddSingleton<QuestionFactoryDelegate>(_ =>
             (text, key, category, isActive) => new Question(isActive, text, key, category));
            services.AddSingleton<IQuestionFactory, QuestionBuilder>();
            services.AddSingleton<ConfigToQuestionMapper>();
            services.AddSingleton<IQdrantClient, QdrantClient>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<RagnarConfig>>().Value.EmbeddingOptions;

                return new QdrantClient(options.Host, options.Port, https: false);
            })
            .AddSingleton<IOllamaClientFactory, OllamaClientFactory>();
            services.AddSingleton<IOutputWriter, AnsiConsoleOutputWriter>();

            // Configuration & Plugins
            //services.ConfigureApplicationOptions(context);
            services.RegisterEmbeddingServices(context);
            services.RegisterOptions(context);
            services.AddHttpClients();
            services.LoadQuestionPlugins();
            services.EmbeddingSetup();
            services.AddHostedService<RagPipelineRunner>();
        })
            .UseSerilog((ctx, config) => config.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

    private static void SetupPrimaryPipeline(IServiceCollection Services)
    {
        Services
        .AddKeyedScoped<IPipelineStage, BrandingStage>("Main");

        Services
        .AddKeyedScoped<IPipelineStage, KnowledgeBasePreparationStage>("Main");
        Services
        .AddKeyedScoped<IPipelineStage, QuestionProcessingStage>("Main");
        Services
        .AddKeyedScoped<IPipelineStage, SummarizationStage>("Main");
    }

}
