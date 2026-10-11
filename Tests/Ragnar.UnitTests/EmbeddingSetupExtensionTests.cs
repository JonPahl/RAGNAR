namespace Ragnar.Tests;

public class EmbeddingSetupExtensionTests
{
    [Fact]
    public void EmbeddingSetupShouldRegisterIEmbeddingService()
    {
        var services = new ServiceCollection();
        services.EmbeddingSetup();

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<IEmbeddingService>();

        Assert.NotNull(service);
    }

    [Fact]
    public void EmbeddingSetupShouldRegisterIQdrantPointFactory()
    {
        var services = new ServiceCollection();
        services.EmbeddingSetup();

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<IQdrantPointFactory>();

        Assert.NotNull(service);
    }

    [Fact]
    public void EmbeddingSetupShouldRegisterIVectorSetup()
    {
        var services = new ServiceCollection();
        services.EmbeddingSetup();

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<IVectorSetup>();

        Assert.NotNull(service);
    }

    [Fact]
    public void EmbeddingSetupShouldRegisterIVectorStoreWriter()
    {
        var services = new ServiceCollection();
        services.EmbeddingSetup();

        var provider = services.BuildServiceProvider();
        var service = provider.GetService<IVectorStoreWriter>();

        Assert.NotNull(service);
    }

    [Fact]
    public void EmbeddingSetupShouldReturnSameServiceCollection()
    {
        var services = new ServiceCollection();
        var result = services.EmbeddingSetup();

        Assert.Same(services, result);
    }
}
