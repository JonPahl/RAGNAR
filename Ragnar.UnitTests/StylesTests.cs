namespace Ragnar.Tests;

public sealed class StylesTests
{
    [Fact]
    public void GreenBlinkHasCorrectColorAndDecoration()
    {
        // Act
        var style = Styles.GreenBlink;

        // Assert
        Assert.Equal(Color.Green, style.Foreground);
        Assert.True(style.Decoration.HasFlag(Decoration.SlowBlink));
    }

    [Fact]
    public void YellowHasCorrectColor()
    {
        // Act
        var style = Styles.Yellow;

        // Assert
        Assert.Equal(Color.Yellow, style.Foreground);
        Assert.Equal(Decoration.None, style.Decoration);
    }

    [Theory]
    [InlineData("", new[] { "file.txt" }, false)]
    public void IsExcludedEdgeCases(string fileName, string[] exclusions, bool expected)
    {
        var result = fileName.AsSpan().IsExcluded(exclusions);
        Assert.Equal(expected, result);
    }
}
