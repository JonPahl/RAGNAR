namespace Ragnar.Extensions;

public static class RagPipelineExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddHttpClients()
        {
            var registerClient = (string name, Action<HttpClient> configure) =>
            {
                services.AddHttpClient(name).AddStandardResilienceHandler(opt =>
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

            registerClient(nameof(OllamaServiceType.Embedding), _ =>
            {

            });

            return services;
        }


        /// <summary>Registers configuration binding and validation for all ollamaOption.</summary>
        /// <param name="context">Host <paramref name="context"/>.</param>
        /// <returns>Updated service collection.</returns>
        public IServiceCollection RegisterOptions(HostBuilderContext context)
        {
            services.AddValidatorsFromAssemblyContaining<RagnarConfig>();

            // Register all services once

            services
                .AddOptions<RagnarConfig>()
                .Bind(context.Configuration
                .GetSection("RagnarConfig"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services
              .PostConfigure<RagnarConfig>(options => options.ApplicationOptions.SourceDirectory = options.ApplicationOptions.SourceDirectory.ExpandDirectory());

            return services;
        }
    }
}
