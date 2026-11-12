namespace Ragnar.Extensions;

/// <summary>Registers embedding-related services and pipelines in the DI container.</summary>
/// <example><![CDATA[builder.Services.EmbeddingSetup();]]></example>
public static class EmbeddingSetupExtension
{
    /// <remarks>Adds OllamaEmbeddingGenerator, PointStructFactory, VectorSetup, and Writer.</remarks>
    /// <param name="services">The IServiceCollection to register services into.</param>
    /// <returns>The same IServiceCollection for further configuration.</returns>
    /// <example><![CDATA[builder.Services.EmbeddingSetup();]]></example>
    extension(IServiceCollection services)
    {

        /// <summary>Registers file-parsing singletons and the parser factory in DI.</summary>
        /// <returns>The same IServiceCollection for further configuration.</returns>
        /// <example>
        /// <![CDATA[builder.Services.CodeParser();]]>
        /// </example>
        public IServiceCollection CodeParser()
        {
            services.AddSingleton<IFileParser>(sp =>
            {
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger>();
                var chunker = sp.GetRequiredService<ISyntaxTreeChunker>();

                return new ParseCSharpFile(logger, chunker);
            });
            services.AddSingleton<IFileParser>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger>();
                var option = sp.GetRequiredService<IOptions<RagnarConfig>>();

                return new MarkdownFileParser(option, logger);
            });

            services.AddSingleton<IFileParseFactory, CodeParserFactory>();

            return services;
        }

        /// <summary>Adds all embedding pipeline singletons and scoped services to DI.</summary>
        /// <returns>The configured IServiceCollection for chaining.</returns>
        /// <example><![CDATA[builder.Services.EmbeddingSetup();]]>
        /// </example>
        public IServiceCollection EmbeddingSetup()
        {
            services.AddSingleton<IEmbeddingService, OllamaEmbeddingGenerator>();

            services.AddSingleton<QuestionFilterSelector>();

            services.AddSingleton<IQdrantPointFactory, PointStructFactory>();

            services
                .AddSingleton<IVectorSetup, VectorSetup>()
                .AddSingleton<IVectorStoreWriter, VectorStoreWriter>();
            return services;
        }
    }
}
