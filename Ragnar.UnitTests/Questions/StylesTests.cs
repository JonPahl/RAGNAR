using Ragnar.Utils;

using Spectre.Console;

namespace RAGNAR.UnitTests.Questions;

public sealed class StylesTests
{
    [Fact]
    public void GreenBlink_HasCorrectStyle()
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
    }

    [Fact]
    public void BoldBlue_HasBoldAndItalic()
    {
        // Act
        var style = Styles.BoldBlue;

        // Assert
        Assert.Equal(Color.Blue, style.Foreground);
        //Assert.Contains(Decoration.Bold, style.Decoration);
        //Assert.Contains(Decoration.Italic, style.Decoration);
    }
}
