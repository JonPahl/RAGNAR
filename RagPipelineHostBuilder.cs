using System.Runtime.Loader;

using Microsoft.Extensions.Http.Resilience;

using Polly;

using Ragnar.Questions.Interface;
using Ragnar.Questions.Questions;

using Serilog;

namespace Ragnar;
/// <summary>Centralized application builder with services, config, and logging.</summary>
public static class RagPipelineHostBuilder
{
    /// <summary>Creates a pre-configured host builder with services, config, and logging.</summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Configured IHostBuilder.</returns>
    /// <example><![CDATA[Host = RagPipelineBuilder.CreateDefaultBuilder(args).Build();]]></example>
    public static IHostBuilder CreateDefaultBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
          .ConfigureServices((context, services) =>
          {
              services.AddSingleton<QuestionFactoryDelegate>(_ =>
                            (text, key, category, isActive) => new Question(isActive, text, key, category));

              services.AddSingleton<IQuestionFactory, DefaultQuestionFactory>();
              services.AddSingleton<ConfigurationBasedQuestionLoader>();
              services.AddSingleton<IQdrantClient, QdrantClient>(serviceProvider =>
                            {
                                var options = serviceProvider.GetRequiredService<IOptions<EmbeddingOptions>>().Value;

                                return new QdrantClient(options.Host, options.Port, https: false);
                            });

              services.RegisterOptions(context);
              services.AddHttpClients()
              .AddSingleton<IOllamaClientProvider, OllamaClientProvider>();
              services.RegisterEmbeddingGenerator(context);

              services.RegisterQuestionPlugins();

              services
              .AddScoped<IResponseWriter, ResponseWriter>()
              .AddScoped<IFileValidator, FileValidator>();
              services.AddKeyedSingleton<ISystemPromptProvider, SystemPromptProvider>("Common")
                .AddKeyedSingleton<ISystemPromptProvider, SummarizePromptProvider>("Summary");

              services.AddSingleton<IKnowledgeBaseInitialize, KnowledgeBaseInitialization>()
              .AddScoped<IAssemblyInfo, AssemblyInfo>()
              .AddSingleton<IApplicationBanner, ApplicationBanner>()
              .AddSingleton<ISummaryService, SummaryService>()

              .AddSingleton<IOutputWriter, AnsiConsoleOutputWriter>()

              .AddSingleton<IRagOrchestrator, RagOrchestrator>()
              .AddScoped<IQuestionEmbedding, QuestionEmbedding>()
              .AddSingleton<IOllamaClientProvider, OllamaClientProvider>()
              .AddSingleton<IOllamaResponse, OllamaResponse>()
              .AddSingleton<IVectorService, VectorStoreProvisioner>()
              .AddSingleton<IQuestionCatalogLoader, DefaultQuestionCatalogLoader>();

              services.EmbeddingSetup();

              services.AddHostedService<RagPipelineRunner>();
          })
          .UseSerilog((ctx, configuration) =>
              configuration
              .ReadFrom.Configuration(ctx.Configuration)
              .WriteTo.Console());
    }

    /// <summary>
    /// Registers embedding generator.
    /// </summary>
    private static IServiceCollection RegisterEmbeddingGenerator(this IServiceCollection services, HostBuilderContext context)
    {
        services.AddSingleton(sp =>
        {
            var embeddingOption = context.Configuration.GetSection("EmbeddingOption")
            .Get<EmbeddingOptions>();

            var ollamaOption = context.Configuration.GetSection("OllamaOption")
            .Get<OllamaOptions>();

            var httpClient = sp.GetRequiredService<IHttpClientFactory>();

            var ollamaHttpClient = httpClient
            .CreateClient(nameof(OllamaType.Ollama));

            var host = ollamaOption?.Host ?? throw new ArgumentException("Ollama Host");

            var port = ollamaOption?.Port ?? throw new ArgumentException("Ollama Port");

            ollamaHttpClient.BaseAddress = new Uri($"{host}:{port}");
            ollamaHttpClient.Timeout = ollamaOption.Timeout;

            var generator = new OllamaApiClient(ollamaHttpClient)
            {
                SelectedModel = embeddingOption?.EmbeddingModel ?? throw new ArgumentException("Embedding Model"),
            };

            return generator.AsEmbeddingGenerator();
        });

        return services;
    }

    /// <summary>
    /// Registers HTTP clients with resilience.
    /// </summary>
    private static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient(nameof(OllamaType.Embedding), _ => { })
            .AddStandardResilienceHandler(opt =>
            {
                opt.TotalRequestTimeout = new HttpTimeoutStrategyOptions()
                {
                    Timeout = TimeSpan.FromMinutes(5),
                };

                opt.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
                {
                    BreakDuration = TimeSpan.FromMinutes(1),
                    MinimumThroughput = 3,
                    SamplingDuration = TimeSpan.FromMinutes(5)
                };
            });

        services.AddHttpClient(nameof(OllamaType.Ollama), _ => { })
            .AddStandardResilienceHandler(opt =>
            {
                opt.TotalRequestTimeout = new HttpTimeoutStrategyOptions()
                {
                    Timeout = TimeSpan.FromMinutes(20),
                };

                opt.Retry = new HttpRetryStrategyOptions
                {
                    Delay = TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(10),
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    OnRetry = (ctx) =>
                    {
                        // Optional: jitter offset
                        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));
                        ctx.RetryDelay.Add(jitter);
                        return ValueTask.CompletedTask;
                    }
                };
                opt.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
                {
                    BreakDuration = TimeSpan.FromMinutes(1),
                    MinimumThroughput = 3,
                    SamplingDuration = TimeSpan.FromMinutes(5)
                };
            });

        return services;
    }

    /// <summary>
    /// Register plugins to load custom questions.
    /// </summary>
    /// <param name="services">service collection. </param>
    /// <returns>updated service collection.</returns>
    private static IServiceCollection RegisterQuestionPlugins(this IServiceCollection services)
    {
        var pluginDir = Path.Combine(AppContext.BaseDirectory, "Questions", "Plugins");

        if (!Directory.Exists(pluginDir))
        {
            return services;
        }

        foreach (var dll in Directory.EnumerateFiles(pluginDir, "*.dll"))
        {
            try
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);

                var providers = assembly
                    .GetTypes()
                    .Where(t => typeof(IQuestionProvider)
                    .IsAssignableFrom(t)
                    && t.IsClass && !t.IsAbstract
                    && t.GetConstructor(Type.EmptyTypes) != null);

                foreach (var type in providers)
                {
                    services.AddTransient(typeof(IQuestionProvider), type);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine(ex.ToString());
                AnsiConsole.WriteLine($"Failed to load plugin assembly: {dll}", dll);
            }
        }

        return services;
    }

    /// <summary>Registers configuration binding and validation for all ollamaOption.</summary>
    /// <param name="services">DI service collection.</param>
    /// <param name="context">Host <paramref name="context"/>.</param>
    /// <returns>Updated service collection.</returns>
    private static IServiceCollection RegisterOptions(this IServiceCollection services, HostBuilderContext context)
    {
        // Register all services once
        services
            .AddOptions<ApplicationOptions>()
            .Bind(context.Configuration
            .GetSection("ApplicationOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<EmbeddingOptions>()
            .Bind(context.Configuration
            .GetSection("EmbeddingOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<FileLoadOptions>()
            .Bind(context.Configuration
            .GetSection("FileLoadOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<OllamaOptions>()
            .Bind(context.Configuration
            .GetSection("OllamaOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .Configure<ApplicationConfiguration>(context.Configuration);
        services
          .PostConfigure<ApplicationConfiguration>(opts => opts.ApplicationOptions.SourceDirectory = opts.ApplicationOptions.SourceDirectory.ExpandDirectory());

        return services;
    }
}
