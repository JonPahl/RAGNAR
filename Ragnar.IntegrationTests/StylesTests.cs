namespace Ragnar.IntegrationTests;

public class StylesTests
{
    [Fact]
    public void GreenBlink_HasCorrectStyle()
    {
        // Act
        var Style = Styles.GreenBlink;

        // Assert
        Assert.Equal(Color.Green, Style.Foreground);
        Assert.Equal(Decoration.SlowBlink, Style.Decoration);
    }

    [Fact]
    public void Cyan_IsBold()
    {
        // Act
        var Style = Styles.Cyan;

        // Assert
        Assert.Equal(Color.Cyan, Style.Foreground);
        Assert.Equal(Decoration.Bold, Style.Decoration);
    }

    [Fact]
    public void BoldSteelBlue_HasBoldAndItalic()
    {
        // Act
        var Style = Styles.BoldSteelBlue;

        // Assert
        Assert.Equal(Color.SteelBlue, Style.Foreground);
    }
}
