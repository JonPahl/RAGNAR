using Microsoft.Extensions.DependencyInjection;

using Ragnar.Core.Interface;
using Ragnar.Embedding.Embedding;
using Ragnar.Embedding.Factory;
using Ragnar.Embedding.Pipeline;
using Ragnar.Embedding.UnitOfWork;

namespace Ragnar.Embedding;

public static class EmbeddingSetupExtension
{
    extension(IServiceCollection services)
    {
        public IServiceCollection EmbeddingSetup()
        {
            services.AddSingleton<IGeneratorService, EmbeddingPointBuilder>()
                .AddScoped<IEmbeddingPipeline, EmbeddingPipeline>()
                .AddScoped<IVectorStoreRepository, VectorStoreRepository>()
                .AddScoped<IEmbedTextPipeline, EmbedTextPipeline>()
                .AddScoped<IFileParseFactory, FileParseFactory>();
            return services;
        }
    }
}
