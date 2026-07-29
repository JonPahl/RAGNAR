namespace Ragnar.UnitTests;

public class StylesTests
{
    [Fact]
    public void GreenBlink_HasCorrectProperties ()
    {
        var style = Styles.GreenBlink;
        Assert.Equal(Color.Green, style.Foreground);
    }
}
