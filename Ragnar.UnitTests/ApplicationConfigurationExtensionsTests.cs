namespace Ragnar.Tests;

// ───────────────────────────────────────────────────────────────
//  2.  ApplicationConfigurationExtensions
// ───────────────────────────────────────────────────────────────
public class ApplicationConfigurationExtensionsTests
{
    //[Fact]
    //public void RegisterEmbeddingServices_RegistersSingleton()
    //{
    //    var services = new ServiceCollection();

    //    // Register required dependencies
    //    services.AddSingleton<Serilog.ILogger>(new SelfLog());
    //    services.AddSingleton<IOptions<RagnarConfig>>(new Mock<IOptions<RagnarConfig>>().Object);
    //    services.AddSingleton<IOllamaClientFactory>(new Mock<IOllamaClientFactory>().Object);

    //    var result = services.RegisterEmbeddingServices();

    //    Assert.Same(services, result);

    //    var provider = services.BuildServiceProvider();
    //    var svc = provider.GetService<IEmbeddingService>();
    //    Assert.NotNull(svc);
    //    Assert.IsType<OllamaEmbeddingService>(svc);
    //}

    //[Fact]
    //public void RegisterEmbeddingServices_ReturnsSameCollection_ForChaining()
    //{
    //    var services = new ServiceCollection();
    //    services.AddSingleton<Serilog.ILogger>(SelfLog());
    //    services.AddSingleton<IOptions<RagnarConfig>>(new Mock<IOptions<RagnarConfig>>().Object);
    //    services.AddSingleton<IOllamaClientFactory>(new Mock<IOllamaClientFactory>().Object);

    //    var result = services.RegisterEmbeddingServices();

    //    Assert.Same(services, result);
    //}

    [Fact]
    public void LoadQuestionPluginsReturnsServicesWhenPluginDirDoesNotExist()
    {
        var services = new ServiceCollection();
        var result = services.LoadQuestionPlugins();

        Assert.Same(services, result);
        // No IQuestionProvider should be registered since the dir won't exist in test env
        var provider = services.BuildServiceProvider();
        Assert.Empty(provider.GetServices<IQuestionProvider>());
    }
}







