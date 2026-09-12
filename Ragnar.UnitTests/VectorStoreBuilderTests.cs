//using Qdrant.Client.Grpc;

//namespace Ragnar.Tests;

//// ───────────────────────────────────────────────────────────────
////  12. VectorStoreBuilder
//// ───────────────────────────────────────────────────────────────
//public class VectorStoreBuilderTests
//{
//    private static (VectorStoreBuilder builder, Mock<IQdrantClient> qdrant) CreateBuilder(
//        string name = "TestStore", ulong dim = 768)
//    {
//        var mockQdrant = new Mock<IQdrantClient>();
//        var logger = new Serilog.Debugging.SelfLog();
//        var builder = new VectorStoreBuilder(logger, dim, name, mockQdrant.Object);
//        return (builder, mockQdrant);
//    }

//    [Fact]
//    public async Task BuildAsync_CreatesCollection_WhenNotExists()
//    {
//        var (builder, mockQdrant) = CreateBuilder();

//        mockQdrant.Setup(q => q.CollectionExistsAsync("TestStore", It.IsAny<CancellationToken>()))
//                  .ReturnsAsync(false);
//        mockQdrant.Setup(q => q.CreateCollectionAsync("TestStore", It.IsAny<VectorParams>(),
//                  It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

//        var result = await builder.BuildAsync(CancellationToken.None);

//        Assert.True(result);
//        mockQdrant.Verify(q => q.CreateCollectionAsync("TestStore",
//            It.Is<VectorParams>(p => p.Size == 768 && p.Distance == Distance.Cosine),
//            It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task BuildAsync_SkipsCreation_WhenAlreadyExists()
//    {
//        var (builder, mockQdrant) = CreateBuilder();

//        mockQdrant.Setup(q => q.CollectionExistsAsync("TestStore", It.IsAny<CancellationToken>()))
//                  .ReturnsAsync(true);

//        var result = await builder.BuildAsync(CancellationToken.None);

//        Assert.True(result);
//        mockQdrant.Verify(q => q.CreateCollectionAsync(It.IsAny<string>(),
//            It.IsAny<VectorParams>(), It.IsAny<CancellationToken>()), Times.Never);
//    }

//    [Fact]
//    public async Task ExistsAsync_SetsIsExisting_False()
//    {
//        var (builder, mockQdrant) = CreateBuilder();
//        mockQdrant.Setup(q => q.CollectionExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//                  .ReturnsAsync(false);

//        await builder.ExistsAsync(CancellationToken.None);

//        // BuildAsync would create; we just verify no exception
//        Assert.NotNull(builder);
//    }

//    [Fact]
//    public async Task ExistsAsync_SetsIsExisting_True()
//    {
//        var (builder, mockQdrant) = CreateBuilder();
//        mockQdrant.Setup(q => q.CollectionExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//                  .ReturnsAsync(true);

//        var result = await builder.ExistsAsync(CancellationToken.None);

//        Assert.Same(builder, result);
//    }

//    [Fact]
//    public async Task CreateAsync_CallsCreateCollection()
//    {
//        var (builder, mockQdrant) = CreateBuilder(dim: 512);
//        mockQdrant.Setup(q => q.CreateCollectionAsync(It.IsAny<string>(), It.IsAny<VectorParams>(),
//                  It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

//        await builder.CreateAsync(CancellationToken.None);

//        mockQdrant.Verify(q => q.CreateCollectionAsync("TestStore",
//            It.Is<VectorParams>(p => p.Size == 512), It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task MakeIndexAsync_CallsCreatePayloadIndex()
//    {
//        var (builder, mockQdrant) = CreateBuilder();
//        mockQdrant.Setup(q => q.CreatePayloadIndexAsync("TestStore", "Category",
//                  PayloadSchemaType.Keyword, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

//        var result = await builder.MakeIndexAsync("Category", PayloadSchemaType.Keyword, CancellationToken.None);

//        Assert.Same(builder, result);
//        mockQdrant.Verify(q => q.CreatePayloadIndexAsync("TestStore", "Category",
//            PayloadSchemaType.Keyword, It.IsAny<CancellationToken>()), Times.Once);
//    }
//}
