namespace Ragnar.Extensions;

/// <summary>Registers embedding-related services and pipelines in the dependency injection container.</summary>
public static class EmbeddingSetupExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection EmbeddingSetup()
        {
            services.AddSingleton<IEmbeddingService, OllamaEmbeddingGenerator>();

            services.AddSingleton<IQdrantPointFactory, PointStructFactory>();

            services
                .AddScoped<IVectorSetup, VectorSetup>()
                .AddScoped<IVectorStoreWriter, VectorStoreWriter>()
                .AddScoped<IFileParseFactory, CodeParserFactory>();
            return services;
        }
    }
}
