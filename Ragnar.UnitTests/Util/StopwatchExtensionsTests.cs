namespace Ragnar.Tests.Util;

public sealed class StopwatchExtensionsTests
{
    [Theory]
    [InlineData(1500)]
    [InlineData(61_000)] // > 1 minute)]
    public void ElapsedTimeString_ReturnsMmSsFormat(int delay)
    {
        // Arrange
        var Sw = new Stopwatch();

        // Act
        Sw.Start();
        Thread.Sleep(delay);
        Sw.Stop();

        var Result = Sw.ElapsedTimeString();

        // Assert
        Result.Should().MatchRegex(@"^\d{2}:\d{2}$");
    }
}
