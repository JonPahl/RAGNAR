namespace Ragnar.UnitTests.Extensions;

public sealed class StringExtensionsTests
{
    [Fact]
    public void CharacterCount_ShouldExcludeXmlTagsAndSlashes ()
    {
        // Arrange
        const string input = "<summary>Test /// comment</summary>";

        // Act
        var count = input.AsSpan().CharacterCount();

        // Assert
        Assert.Equal(13, count);
    }

    [Theory]
    [InlineData("<c>code</c>", 4)]
    [InlineData("Hello <!-- comment -->", 6)]
    [InlineData("<summary>Summary text</summary>", 12)]
    [InlineData("", 0)]
    public void CharacterCount_ExcludesTagsAndSlashes (string xml, int expected)
    {
        // Act
        var count = xml.CharacterCount();

        // Assert
        Assert.Equal(expected, count);
    }
}
