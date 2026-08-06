using Ragnar.Embedding;

namespace Ragnar.IntegrationTests;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("<summary>Test</summary>", 4)] // only 'Test'
    [InlineData("<!-- comment -->", 0)]
    [InlineData("plain text", 10)]
    public void CharacterCount_Should_Count_NonTag_NonComment(string Input, int Expected)
    {
        var result = Input.AsSpan().CharacterCount();
        Assert.Equal(Expected, result);
    }
}
