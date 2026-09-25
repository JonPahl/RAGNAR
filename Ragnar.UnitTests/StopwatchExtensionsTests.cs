namespace Ragnar.Tests;

public class StopwatchExtensionsTests
{
    [Fact]
    public void ElapsedTimeStringStoppedWatchShouldReturnZeroSeconds()
    {
        // Arrange
        var sw = Stopwatch.StartNew();
        sw.Stop();

        // Act
        var result = sw.FormatElapsedTime();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("00:00", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public async Task ElapsedTimeStringRunningWatchShouldReturnFormattedTime()
    {
        // Arrange
        var sw = Stopwatch.StartNew();
        await Task.Delay(1500, TestContext.Current.CancellationToken);

        sw.Stop();

        // Act
        var result = sw.FormatElapsedTime();

        // Assert
        Assert.Matches(@"^\d{2}:\d{2}$", result);
        Assert.StartsWith("00:01", result, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public void ElapsedTimeStringShouldHaveExactlyFiveCharacters()
    {
        // Arrange
        var sw = Stopwatch.StartNew();
        sw.Stop();

        // Act
        var result = sw.FormatElapsedTime();

        // Assert
        Assert.Equal(5, result.Length);
        Assert.Contains(':', result);
    }
}
