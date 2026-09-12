namespace Ragnar.Embedding.Pipeline;


/// <summary>Provides extension methods for DI service registration.</summary>
/// <remarks>Encapsulates embedding and plugin discovery registrations.</remarks>
/// <example><![CDATA[services.RegisterEmbeddingServices().LoadQuestionPlugins();]]></example>
public static class ApplicationConfigurationExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>Registers the Ollama-based embedding service as a singleton.</summary>
        /// <remarks>Resolves logger, config, and client factory from the container.</remarks>        
        /// <returns>The service collection for chaining.</returns>
        /// <example><![CDATA[services.RegisterEmbeddingServices();]]></example>
        public IServiceCollection RegisterEmbeddingServices()
        {
            services.AddSingleton<IEmbeddingService>(sp =>
            {
                var logger = sp.GetService<Serilog.ILogger>();
                var config = sp.GetService<IOptions<RagnarConfig>>();

                var ollamaClientProvider = sp.GetRequiredService<IOllamaClientFactory>();

                return new OllamaEmbeddingService(logger, ollamaClientProvider, config);
            });

            return services;
        }


        /// <summary>Discovers and registers IQuestionProvider implementations from plugin DLLs.</summary>
        /// <remarks>Loads assemblies from the base Questions/Plugins directory.</remarks>
        /// <example><![CDATA[services.LoadQuestionPlugins();]]></example>
        /// <returns>The service collection for chaining.</returns>
        public IServiceCollection LoadQuestionPlugins()
        {
            var pluginDir = Path.Join(AppContext.BaseDirectory, "Questions", "Plugins");
            if (!Directory.Exists(pluginDir)) return services;

            foreach (var dll in Directory.EnumerateFiles(pluginDir, "*.dll"))
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);

                    var providers = assembly.GetTypes()
                        .Where(t => typeof(IQuestionProvider).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

                    foreach (var type in providers) services.AddTransient(typeof(IQuestionProvider), type);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to load plugin: {dll}[/]");
                    AnsiConsole.MarkupLine($"[dim]{ex.Message}[/]");
                }
            }
            return services;
        }
    }
}
