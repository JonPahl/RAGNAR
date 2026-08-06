namespace Ragnar.UnitTests.Extensions;

public sealed class StringExtensionsTests
{
    [Fact]
    public void CharacterCountShouldExcludeXmlTagsAndSlashes()
    {
        // Arrange
        const string Input = "<summary>Test /// comment</summary>";

        // Act
        var Count = Input.AsSpan().CharacterCount();

        // Assert
        Assert.Equal(13, Count);
    }

    [Theory]
    [InlineData("<c>code</c>", 4)]
    [InlineData("Hello <!-- comment -->", 6)]
    [InlineData("<summary>Summary text</summary>", 12)]
    [InlineData("", 0)]
    public void CharacterCountExcludesTagsAndSlashes(string Xml, int Expected)
    {
        // Act
        var Count = Xml.CharacterCount();

        // Assert
        Assert.Equal(Expected, Count);
    }
}
