namespace Ragnar.UnitTests;

public sealed class StylesTests
{

    [Fact]
    public void GreenBlinkHasCorrectColorAndDecoration()
    {
        // Act
        var Style = Styles.GreenBlink;

        // Assert
        Assert.Equal(Color.Green, Style.Foreground);
        Assert.Equal(Decoration.SlowBlink, Style.Decoration);
    }


    [Fact]
    public void GreenBlinkHasCorrectStyle()
    {
        // Act
        var Style = Styles.GreenBlink;

        // Assert
        Assert.Equal(Color.Green, Style.Foreground);
        Assert.Equal(Decoration.SlowBlink, Style.Decoration);
    }

    [Fact]
    public void YellowHasCorrectColor()
    {
        // Act
        var Style = Styles.Yellow;

        // Assert
        Assert.Equal(Color.Yellow, Style.Foreground);
    }

    [Fact]
    public void BoldBlueHasBoldAndItalic()
    {
        // Act
        var Style = Styles.BoldBlue;

        // Assert
        Assert.Equal(Color.Blue, Style.Foreground);
        Assert.Equal(Decoration.Bold | Decoration.Italic, Style.Decoration);
    }
}
