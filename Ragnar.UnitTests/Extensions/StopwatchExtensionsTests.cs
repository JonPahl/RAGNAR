namespace Ragnar.Tests.Extensions;

public class StopwatchExtensionsTests
{
    [Theory]
    [InlineData(1500, "00:01")]
    [InlineData(65432, "01:05")]
    public void ElapsedTimeString_ReturnsFormattedTime(int Delay, string Expected)
    {
        // Arrange
        var sw = new Stopwatch();
        sw.Start();
        Thread.Sleep(Delay);
        sw.Stop();

        // Act
        var result = sw.ElapsedTimeString();

        // Assert
        result.Should().MatchRegex(@"^\d{2}:\d{2}$");
        result.Should().Contain(Expected);
    }
}
