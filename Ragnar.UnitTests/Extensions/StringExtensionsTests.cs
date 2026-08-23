namespace Ragnar.Tests.Extensions;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("<c>code</c>", 4)]
    [InlineData("Hello <!-- comment -->", 5)]
    [InlineData("<summary>Summary text</summary>", 11)]
    [InlineData("", 0)]
    public void CharacterCount_ExcludesTagsAndSlashes(string xml, int expected)
    {
        // Act
        var count = xml.CharacterCount();

        // Assert
        Assert.Equal(expected, count);
    }
}
