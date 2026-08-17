namespace Ragnar.Embedding;

/// <summary>Registers embedding-related services and pipelines in the dependency injection container.</summary>
public static class EmbeddingSetupExtension
{
    extension(IServiceCollection services)
    {
        /// <returns>The updated service collection with all registered embedding dependencies.</returns>
        /// <example><![CDATA[services.EmbeddingSetup();]]></example>
        public IServiceCollection EmbeddingSetup()
        {
            services.AddSingleton<IGeneratorService, EmbeddingPointFactory>()
                .AddScoped<IEmbeddingPipeline, EmbeddingPipeline>()
                .AddScoped<IVectorStoreRepository, VectorStoreRepository>()
                .AddScoped<IEmbedTextPipeline, EmbedTextPipeline>()
                .AddScoped<IFileParseFactory, FileParseFactory>();
            return services;
        }
    }
}
