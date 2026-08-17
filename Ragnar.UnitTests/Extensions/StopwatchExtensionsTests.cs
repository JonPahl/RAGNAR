namespace Ragnar.UnitTests.Extensions;

public class StopwatchExtensionsTests
{
    [Fact]
    public void ElapsedTimeString_ReturnsMmSsFormat()
    {
        var sw = new Stopwatch();
        sw.Start();
        Thread.Sleep(1200); // ~1.2 seconds -> "00:01"
        sw.Stop();

        var result = sw.ElapsedTimeString();
        Assert.Matches(@"^\d{2}:\d{2}$", result);
    }
}
