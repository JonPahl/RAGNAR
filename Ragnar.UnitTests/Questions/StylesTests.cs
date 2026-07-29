




namespace RAGNAR.UnitTests.Questions;

public sealed class StylesTests
{

    [Fact]
    public void GreenBlink_HasCorrectColorAndDecoration()
    {
        // Act
        var style = Styles.GreenBlink;

        // Assert
        Assert.Equal(Color.Green, style.Foreground);
        Assert.Equal(Decoration.SlowBlink, style.Decoration);
    }

    [Theory]
    //[InlineData("file.txt", Array.Empty<string>(), false)]
    [InlineData("", new[] { "file.txt" }, false)]
    public void IsExcluded_EdgeCases(string fileName, string[] exclusions, bool expected)
    {
        var result = fileName.AsSpan().IsExcluded(exclusions);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GreenBlink_HasCorrectStyle()
    {
        // Act
        var style = Styles.GreenBlink;

        // Assert
        Assert.Equal(Color.Green, style.Foreground);
        Assert.Equal(Decoration.SlowBlink, style.Decoration);
    }

    [Fact]
    public void Yellow_HasCorrectColor()
    {
        // Act
        var style = Styles.Yellow;

        // Assert
        Assert.Equal(Color.Yellow, style.Foreground);
    }

    [Fact]
    public void BoldBlue_HasBoldAndItalic()
    {
        // Act
        var style = Styles.BoldBlue;

        // Assert
        Assert.Equal(Color.Blue, style.Foreground);
        Assert.Equal(Decoration.Bold | Decoration.Italic, style.Decoration);
    }
}
