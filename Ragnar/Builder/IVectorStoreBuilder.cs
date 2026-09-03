namespace Ragnar.Builder;

public interface IVectorStoreBuilder
{
    ValueTask<bool> BuildAsync(CancellationToken cancellationToken = default);
    ValueTask<IVectorStoreBuilder> MakeIndexAsync(string indexName, PayloadSchemaType schemaType, CancellationToken cancellationToken = default);
}
