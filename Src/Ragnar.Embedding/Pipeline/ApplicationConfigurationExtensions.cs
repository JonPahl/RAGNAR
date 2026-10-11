namespace Ragnar.Embedding.Pipeline;

/// <summary>Provides extension methods for DI service registration.</summary>
/// <remarks>Encapsulates embedding and plugin discovery registrations.</remarks>
/// <example><![CDATA[services.RegisterEmbeddingServices().LoadQuestionPlugins();]]></example>
public static class ApplicationConfigurationExtensions
{
    extension(IServiceCollection services)
    {

        /// <summary>Discovers and registers IQuestionSource implementations from plugin DLLs.</summary>
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
                        .Where(t => typeof(IQuestionSource).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

                    foreach (var type in providers) services.AddTransient(typeof(IQuestionSource), type);
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
