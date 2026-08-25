namespace Ragnar.Embedding.Pipeline;

public static class ApplicationConfigurationExtensions
{
    extension(IServiceCollection Services)
    {
        //public IServiceCollection ConfigureApplicationOptions(HostBuilderContext Context)
        //{
        //    var optionTypes = new[]
        //    {
        //        typeof(ApplicationOptions),
        //        typeof(EmbeddingOptions),
        //        typeof(FileLoadOptions),
        //        typeof(OllamaOptions)
        //    };
        //    return Services;
        //}

        public IServiceCollection RegisterEmbeddingServices(HostBuilderContext Context)
        {
            Services.AddSingleton<IEmbeddingService>(sp =>
            {
                var logger = sp.GetService<Serilog.ILogger>();

                var ollamaClientProvider = sp.GetRequiredService<IOllamaClientFactory>();

                return new OllamaEmbeddingService(logger, ollamaClientProvider);
            });

            return Services;
        }

        public IServiceCollection LoadQuestionPlugins()
        {
            var pluginDir = Path.Join(AppContext.BaseDirectory, "Questions", "Plugins");
            if (!Directory.Exists(pluginDir)) return Services;

            foreach (var dll in Directory.EnumerateFiles(pluginDir, "*.dll"))
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);

                    var providers = assembly.GetTypes()
                    .Where(t => typeof(IQuestionProvider).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

                    foreach (var type in providers) Services.AddTransient(typeof(IQuestionProvider), type);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Failed to load plugin: {dll}[/]");
                    AnsiConsole.MarkupLine($"[dim]{ex.Message}[/]");
                }
            }
            return Services;
        }
    }
}
