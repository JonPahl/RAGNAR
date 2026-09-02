namespace Ragnar.Extensions;

/// <summary>Registers embedding-related services and pipelines in the dependency injection container.</summary>
public static class EmbeddingSetupExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection EmbeddingSetup()
        {
            services.AddSingleton<IEmbeddingService, OllamaEmbeddingService>();

            services.AddSingleton<IGeneratorService, PointStructFactory>();

            services
                .AddScoped<IEmbeddingPipeline, EmbeddingPipeline>()
                .AddScoped<Embedding.UnitOfWork.IVectorStoreRepository, VectorStoreRepository>()
                .AddScoped<IEmbedTextPipeline, EmbedTextPipeline>()
                .AddScoped<IFileParseFactory, CodeParserFactory>();
            return services;
        }
    }
}
