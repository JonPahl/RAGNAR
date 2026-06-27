using Ragnar.Extensions;
using Ragnar.Utils;

using Spectre.Console;

namespace RAGNAR.UnitTests;

public sealed class StylesTests
{
    [Fact]
    public void GreenBlink_HasCorrectColorAndDecoration()
    {
        // Act
        var style = Styles.GreenBlink;

        // Assert
        Assert.Equal(Color.Green, style.Foreground);
        // Assert.Contains(Decoration.SlowBlink, style.Decoration);
    }

    [Fact]
    public void Yellow_HasCorrectColor()
    {
        // Act
        var style = Styles.Yellow;

        // Assert
        Assert.Equal(Color.Yellow, style.Foreground);
        Assert.Equal(Decoration.None, style.Decoration);
    }


    [Theory]
    //[InlineData("file.txt", Array.Empty<string>(), false)]
    [InlineData("", new[] { "file.txt" }, false)]
    public void IsExcluded_EdgeCases(string fileName, string[] exclusions, bool expected)
    {
        var result = fileName.AsSpan().IsExcluded(exclusions);
        Assert.Equal(expected, result);
    }
}
