namespace Ragnar.UnitTests.Util;

public sealed class StopwatchExtensionsTests
{
    [Fact]
    public void ElapsedTimeString_FormatsAs_mm_ss_WithLeadingZero()
    {
        // Arrange
        var sw = new Stopwatch();
        sw.Start();
        Thread.Sleep(100);
        sw.Stop();

        // Act
        var Formatted = sw.ElapsedTimeString();

        // Assert
        Assert.Matches(@"^00:\d{2}$", Formatted);
    }

    [Fact]
    public void ElapsedTimeString_HandlesLargeValues()
    {
        // Arrange
        var sw = new Stopwatch();
        sw.Start();
        Thread.Sleep(61_000); // > 1 minute
        sw.Stop();

        // Act
        var Formatted = sw.ElapsedTimeString();

        // Assert
        Assert.Matches(@"^\d{2}:\d{2}$", Formatted);
    }
}
