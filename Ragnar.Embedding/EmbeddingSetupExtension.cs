namespace Ragnar.Embedding;

public static class EmbeddingSetupExtension
{
    /// <summary>EmbeddingSetupExtension.cs EmbeddingSetup registers embedding services.</summary>
    /// <returns>The updated service collection.</returns>
    extension(IServiceCollection Services)
    {
        /// <summary>Registers embedding services in DI container.</summary>
        /// <returns>Updated service collection.</returns>
        /// <example><![CDATA[services.EmbeddingSetup();]]></example>
        public IServiceCollection EmbeddingSetup()
        {
            Services.AddSingleton<IGeneratorService, EmbeddingPointBuilder>()
                .AddScoped<IEmbeddingPipeline, EmbeddingPipeline>()
                .AddScoped<IVectorStoreWriter, CodeDocumentParserFactory>()
                .AddScoped<ICodeEmbeddingPipeline, CodeEmbeddingPipeline>()
                .AddScoped<IFileParserSelector, FileParserSelector>();
            return Services;
        }
    }
}
