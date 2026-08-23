namespace Ragnar.Tests;

public class SystemPromptProviderTests
{
    [Fact]
    public void Template_Returns_NonEmpty_Prompt()
    {
        var provider = new SystemPromptProvider();
        provider.Template.Should().NotBeNullOrEmpty();
        provider.Template.Should().Contain(".NET 10");
        provider.Template.Should().Contain("C# 14");
    }
}
