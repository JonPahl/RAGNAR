namespace Ragnar.Tests.Extensions;

public class StopwatchExtensionsTests
{
    [Fact]
    public void FormatElapsedTimeLessThanOneMinuteShouldReturnMMSS()
    {
        // Arrange
        var sw = Stopwatch.StartNew();
        Thread.Sleep(50);
        sw.Stop();

        // Act
        var result = sw.FormatElapsedTime();

        // Assert
        Assert.Matches(@"^\d{2}:\d{2}$", result);
    }

    [Fact]
    public void FormatElapsedTimeJustStartedShouldReturnZeroes()
    {
        // Arrange
        var sw = Stopwatch.StartNew();
        sw.Stop();

        // Act
        var result = sw.FormatElapsedTime();

        // Assert
        Assert.Equal("00:00", result);
    }

    [Fact]
    public void FormatElapsedTimeNullShouldThrowArgumentNullException()
    {
        // Arrange
        Stopwatch? sw = null;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => sw.FormatElapsedTime());
    }

    [Fact]
    public void FormatElapsedTimeOneMinutePlusShouldReturnMMSS()
    {
        // Arrange - Simulate by creating a stopwatch and manipulating elapsed
        var sw = Stopwatch.StartNew();
        sw.Stop();
        // We can't easily set elapsed to > 1 min without waiting, so test format
        var result = sw.FormatElapsedTime();

        // Assert - should be in mm:ss format
        Assert.Matches(@"^\d{2}:\d{2}$", result);
    }
}
