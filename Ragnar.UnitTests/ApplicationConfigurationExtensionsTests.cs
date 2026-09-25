namespace Ragnar.Tests;

public class ApplicationConfigurationExtensionsTests
{
    [Fact]
    public void LoadQuestionPluginsReturnsServicesWhenPluginDirDoesNotExist()
    {
        var services = new ServiceCollection();
        var result = services.LoadQuestionPlugins();

        Assert.Same(services, result);
        // No IQuestionSource should be registered since the dir won't exist in test env
        var provider = services.BuildServiceProvider();
        Assert.Empty(provider.GetServices<IQuestionSource>());
    }
}







