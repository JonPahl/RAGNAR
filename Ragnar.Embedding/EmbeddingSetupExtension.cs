namespace Ragnar.Embedding;

public static class EmbeddingSetupExtension
{
    /// <summary>EmbeddingSetupExtension.cs EmbeddingSetup registers embedding services.</summary>
    /// <returns>The updated service collection.</returns>
    extension(IServiceCollection services)
    {
        /// <summary>EmbeddingSetupExtension.cs EmbeddingSetup registers embedding services.
        /// </summary>
        /// <returns>The updated service collection.</returns>
        /// <example><![CDATA[services.EmbeddingSetup();]]></example>
        public IServiceCollection EmbeddingSetup ()
        {
            services.AddSingleton<IGeneratorService, EmbeddingPointBuilder>()
                .AddScoped<IEmbeddingPipeline, EmbeddingPipeline>()
                .AddScoped<IVectorStoreWriter, VectorStoreWriter>()
                .AddScoped<ICodeEmbeddingPipeline, CodeEmbeddingPipeline>()
                .AddScoped<IFileParseFactory, FileParseFactory>();
            return services;
        }
    }
}
