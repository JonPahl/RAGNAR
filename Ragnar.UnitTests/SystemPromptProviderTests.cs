namespace Ragnar.Tests;

public class SystemPromptProviderTests
{
    [Fact]
    public void Template_ReturnsNonEmptyString()
    {
        var provider = new SystemPromptProvider();
        Assert.NotNull(provider.Template);
        Assert.NotEmpty(provider.Template);
        Assert.Contains(".NET 10", provider.Template);
        Assert.Contains("C# 14", provider.Template);
    }

    [Fact]
    public void Content_DefaultsToEmptyString()
    {
        var provider = new SystemPromptProvider();
        Assert.Equal(string.Empty, provider.Content);
    }
}
