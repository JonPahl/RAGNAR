namespace Ragnar.UnitTests.Questions;

public sealed class StylesTests
{
    [Fact]
    public void GreenBlink_HasCorrectStyle()
    {
        // Act
        var Style = Styles.GreenBlink;

        // Assert
        Assert.Equal(Color.Green, Style.Foreground);

        Style.Decoration.HasFlag(Decoration.SlowBlink).Should().BeTrue();
    }

    [Fact]
    public void Yellow_HasCorrectColor()
    {
        // Act
        var Style = Styles.Yellow;

        // Assert
        Assert.Equal(Color.Yellow, Style.Foreground);
    }

    [Fact]
    public void BoldBlue_HasBoldAndItalic()
    {
        // Act
        var style = Styles.BoldBlue;

        // Assert
        Assert.Equal(Color.Blue, style.Foreground);
        Assert.True(style.Decoration.HasFlag(Decoration.Bold));
        Assert.True(style.Decoration.HasFlag(Decoration.Italic));
    }
}
