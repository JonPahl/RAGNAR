namespace Ragnar;

/// <summary>Centralized application builder with services, config, and logging.</summary>
public static class RagPipelineHostBuilder
{
    /// <summary>Creates a pre-configured host builder with services, config, and logging.</summary>
    /// <param name="Args">Command-line arguments.</param>
    /// <returns>Configured IHostBuilder.</returns>
    /// <example><![CDATA[Host = RagPipelineBuilder.CreateDefaultBuilder(args).ToCollection();]]></example>
    public static IHostBuilder CreateDefaultBuilder(string[] Args)
    {
        return Host.CreateDefaultBuilder(Args)
          .ConfigureServices((Context, Services) =>
          {
              Services.AddSingleton<QuestionFactoryDelegate>(_ =>
                            (Text, Key, Category, IsActive) => new Question(IsActive, Text, Key, Category));

              Services.AddSingleton<IQuestionFactory, DefaultQuestionFactory>();
              Services.AddSingleton<ConfigToQuestionMapper>();
              Services.AddSingleton<IQdrantClient, QdrantClient>(ServiceProvider =>
              {
                  var Options = ServiceProvider.GetRequiredService<IOptions<EmbeddingOptions>>().Value;

                  return new QdrantClient(Options.Host, Options.Port, https: false);
              });

              Services.AddScoped<IVectorStore, QdrantVectorStore>();

              Services.RegisterOptions(Context);
              Services.AddHttpClients()
              .AddSingleton<IOllamaClientFactory, OllamaClientProvider>();
              Services.RegisterEmbeddingGenerator(Context);

              Services.RegisterQuestionPlugins();

              Services
              .AddScoped<IResponseWriter, ResponseWriter>()
              .AddScoped<IFileValidator, FileValidator>();
              Services.AddKeyedSingleton<ISystemPromptProvider, SystemPromptProvider>("Common")
                .AddKeyedSingleton<ISystemPromptProvider, SummarizePromptProvider>("Summary");

              Services.AddSingleton<IKnowledgeBaseInitialize, KnowledgeBaseInitialization>()
              .AddScoped<IAssemblyInfo, AssemblyInfo>()
              .AddSingleton<IApplicationBanner, ApplicationBanner>()
              .AddSingleton<ISummaryService, SummaryService>()
              .AddSingleton<IOutputWriter, AnsiConsoleOutputWriter>()
              .AddSingleton<ICodeAnalysisPipeline, CodeAnalysisPipeline>()
              .AddScoped<ICustomEmbedding, QuestionEmbedding>()
              .AddSingleton<IOllamaClientFactory, OllamaClientProvider>()
              .AddSingleton<IOllamaResponse, OllamaResponse>()
              .AddSingleton<IQuestionCatalogLoader, DefaultQuestionCatalogLoader>();

              Services.EmbeddingSetup();
              Services.AddHostedService<RagPipelineRunner>();
          })
          .UseSerilog((Ctx, Configuration) =>
              Configuration
              .ReadFrom.Configuration(Ctx.Configuration)
              .WriteTo.Console());
    }

    /// <summary>
    /// Registers embedding generator.
    /// </summary>
    private static IServiceCollection RegisterEmbeddingGenerator(
        this IServiceCollection Services, HostBuilderContext Context)
    {
        Services.AddSingleton(Sp =>
        {
            var EmbeddingOption = Context.Configuration.GetSection("EmbeddingOption").Get<EmbeddingOptions>();

            var OllamaOption = Context.Configuration.GetSection("OllamaOption").Get<OllamaOptions>();

            var HttpClient = Sp.GetRequiredService<IHttpClientFactory>();

            var OllamaHttpClient = HttpClient
            .CreateClient(nameof(OllamaServiceType.Ollama));

            var Host = OllamaOption?.Host ?? throw new ArgumentException("Ollama Host");

            var Port = OllamaOption?.Port ?? throw new ArgumentException("Ollama Port");

            OllamaHttpClient.BaseAddress = new Uri($"{Host}:{Port}");
            OllamaHttpClient.Timeout = OllamaOption.Timeout;

            var Generator = new OllamaApiClient(OllamaHttpClient)
            {
                SelectedModel = EmbeddingOption?.EmbeddingModel ?? throw new ArgumentException("Embedding Model"),
            };

            return Generator.AsEmbeddingGenerator();
        });

        return Services;
    }

    /// <summary>
    /// Registers HTTP clients with resilience.
    /// </summary>
    private static IServiceCollection AddHttpClients(this IServiceCollection Services)
    {
        Services.AddHttpClient(nameof(OllamaServiceType.Embedding), _ => { })
            .AddStandardResilienceHandler(Opt =>
            {
                Opt.TotalRequestTimeout = new HttpTimeoutStrategyOptions()
                {
                    Timeout = TimeSpan.FromMinutes(5),
                };

                Opt.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
                {
                    BreakDuration = TimeSpan.FromMinutes(1),
                    MinimumThroughput = 3,
                    SamplingDuration = TimeSpan.FromMinutes(5)
                };
            });

        Services.AddHttpClient(nameof(OllamaServiceType.Ollama), _ => { })
            .AddStandardResilienceHandler(Opt =>
            {
                Opt.TotalRequestTimeout = new HttpTimeoutStrategyOptions()
                {
                    Timeout = TimeSpan.FromMinutes(20),
                };

                Opt.Retry = new HttpRetryStrategyOptions
                {
                    Delay = TimeSpan.FromSeconds(2),
                    MaxDelay = TimeSpan.FromSeconds(10),
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    OnRetry = (Ctx) =>
                    {
                        var Jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));
                        Ctx.RetryDelay.Add(Jitter);
                        return ValueTask.CompletedTask;
                    }
                };

                Opt.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
                {
                    BreakDuration = TimeSpan.FromMinutes(1),
                    MinimumThroughput = 3,
                    SamplingDuration = TimeSpan.FromMinutes(5)
                };
            });

        return Services;
    }

    /// <summary>
    /// Register plugins to load custom questions.
    /// </summary>
    /// <param name="Services">service collection. </param>
    /// <returns>updated service collection.</returns>
    private static IServiceCollection RegisterQuestionPlugins(this IServiceCollection Services)
    {
        var PluginDir = Path.Combine(AppContext.BaseDirectory, "Questions", "Plugins");

        if(!Directory.Exists(PluginDir))
        {
            return Services;
        }

        foreach(var Dll in Directory.EnumerateFiles(PluginDir, "*.dll"))
        {
            try
            {
                var Assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Dll);

                var Providers = Assembly
                    .GetTypes()
                    .Where(T => typeof(IQuestionProvider)
                    .IsAssignableFrom(T)
                    && T.IsClass && !T.IsAbstract
                    && T.GetConstructor(Type.EmptyTypes) != null);

                foreach(var Provider in Providers)
                {
                    Services.AddTransient(typeof(IQuestionProvider), Provider);
                }
            }
            catch(Exception Ex)
            {
                AnsiConsole.WriteLine(Ex.ToString());
                AnsiConsole.WriteLine($"Failed to load plugin assembly: {Dll}", Dll);

                //TODO: insert reference to serilog here and log above item.
            }
        }

        return Services;
    }

    /// <summary>Registers configuration binding and validation for all ollamaOption.</summary>
    /// <param name="Services">DI service collection.</param>
    /// <param name="Context">Host <paramref name="Context"/>.</param>
    /// <returns>Updated service collection.</returns>
    private static IServiceCollection RegisterOptions(this IServiceCollection Services, HostBuilderContext Context)
    {
        // Register all services once
        Services
            .AddOptions<RagOptions>()
            .Bind(Context.Configuration
            .GetSection("RagOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .Validate(Rag => !string.IsNullOrWhiteSpace(Rag.SourceDirectory),
              "RagOptions.SourceDirectory must not be empty")
            .Validate(Rag => Directory.Exists(Rag.SourceDirectory.ExpandDirectory()),
              "RagOptions.SourceDirectory does not exist");

        Services
            .AddOptions<EmbeddingOptions>()
            .Bind(Context.Configuration
            .GetSection("EmbeddingOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        Services
            .AddOptions<FileLoadOptions>()
            .Bind(Context.Configuration
            .GetSection("FileLoadOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        Services
            .AddOptions<OllamaOptions>()
            .Bind(Context.Configuration
            .GetSection("OllamaOptions"))
            .ValidateDataAnnotations()
            .ValidateOnStart()
            .PostConfigure(Option =>
            {
                Option.Host = Option.Host.ValidateHost();
                Option.Port = Option.Port.ValidatePort();
            });

        Services
            .Configure<AppConfiguration>(Context.Configuration)
            .PostConfigure<AppConfiguration>(Opts => Opts.RagOptions.SourceDirectory = Opts.RagOptions.SourceDirectory.ExpandDirectory());

        return Services;
    }
}
