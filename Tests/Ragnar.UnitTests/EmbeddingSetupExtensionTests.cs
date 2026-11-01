namespace Ragnar.Tests;

public class EmbeddingSetupExtensionTests
{
    [Fact]
    public void EmbeddingSetupShouldRegisterIEmbeddingService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.EmbeddingSetup();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEmbeddingService));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(OllamaEmbeddingGenerator), descriptor.ImplementationType);
    }

    [Fact]
    public void EmbeddingSetupShouldRegisterQuestionFilterSelector()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.EmbeddingSetup();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(QuestionFilterSelector));
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void EmbeddingSetupShouldRegisterIQdrantPointFactory()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.EmbeddingSetup();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IQdrantPointFactory));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(PointStructFactory), descriptor.ImplementationType);
    }

    [Fact]
    public void EmbeddingSetupShouldRegisterIVectorSetup()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.EmbeddingSetup();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IVectorSetup));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(VectorSetup), descriptor.ImplementationType);
    }

    [Fact]
    public void EmbeddingSetupShouldRegisterIVectorStoreWriter()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.EmbeddingSetup();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IVectorStoreWriter));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(VectorStoreWriter), descriptor.ImplementationType);
    }

    [Fact]
    public void EmbeddingSetupShouldRegisterAllServicesAsSingletons()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.EmbeddingSetup();

        // Assert
        var serviceTypes = new[]
        {
            typeof(IEmbeddingService),
            typeof(QuestionFilterSelector),
            typeof(IQdrantPointFactory),
            typeof(IVectorSetup),
            typeof(IVectorStoreWriter)
        };

        foreach (var st in serviceTypes)
        {
            var descriptor = services.First(d => d.ServiceType == st);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }
    }

    [Fact]
    public void EmbeddingSetupShouldReturnSameServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.EmbeddingSetup();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void EmbeddingSetupShouldBeIdempotent()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.EmbeddingSetup();
        services.EmbeddingSetup();

        // Assert – no crash, descriptors still resolvable
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<IEmbeddingService>());
        Assert.NotNull(provider.GetRequiredService<IQdrantPointFactory>());
    }
}
