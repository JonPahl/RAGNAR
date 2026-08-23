namespace Ragnar.Extensions;

public static class RagPipelineExtensions
{
    extension(IServiceCollection Services)
    {
        public IServiceCollection AddHttpClients()
        {
            var registerClient = (string name, Action<HttpClient> configure) =>
            {
                Services.AddHttpClient(name).AddStandardResilienceHandler(opt =>
                {
                    opt.TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromMinutes(5) };
                    opt.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
                    {
                        BreakDuration = TimeSpan.FromMinutes(1),
                        MinimumThroughput = 3,
                        SamplingDuration = TimeSpan.FromMinutes(5)
                    };
                });
            };

            registerClient(nameof(OllamaServiceType.Embedding), _ => { });

            //Services.AddHttpClient(nameof(OllamaServiceType.Ollama))
            //    .AddStandardResilienceHandler(opt =>
            //{
            //    opt.TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = TimeSpan.FromMinutes(20) };
            //    opt.Retry = new HttpRetryStrategyOptions
            //    {
            //        Delay = TimeSpan.FromSeconds(2),
            //        MaxDelay = TimeSpan.FromSeconds(20),
            //        MaxRetryAttempts = 3,
            //        BackoffType = DelayBackoffType.Exponential,
            //        OnRetry = ctx =>
            //        {
            //            var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));
            //            ctx.RetryDelay.Add(jitter);
            //            return ValueTask.CompletedTask;
            //        }
            //    };
            //    opt.CircuitBreaker = new HttpCircuitBreakerStrategyOptions
            //    {
            //        BreakDuration = TimeSpan.FromMinutes(1),
            //        MinimumThroughput = 3,
            //        SamplingDuration = TimeSpan.FromMinutes(5)
            //    };
            //});

            return Services;
        }


        /// <summary>Registers configuration binding and validation for all ollamaOption.</summary>
        /// <param name="Context">Host <paramref name="Context"/>.</param>
        /// <returns>Updated service collection.</returns>
        public IServiceCollection RegisterOptions(HostBuilderContext Context)
        {
            Services.AddValidatorsFromAssemblyContaining<RagnarConfig>();

            // Register all services once
            Services
                .AddOptions<RagnarConfig>()
                .Bind(Context.Configuration
                .GetSection("RagnarConfig"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            Services
              .PostConfigure<RagnarConfig>(options => options.ApplicationOptions.SourceDirectory = options.ApplicationOptions.SourceDirectory.ExpandDirectory());

            return Services;
        }
    }
}
