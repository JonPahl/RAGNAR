namespace RAGNAR.UnitTests.Util;

public sealed class StopwatchExtensionsTests
{

    [Fact]
    public void ElapsedTimeStringFormatsAsMinutesAndSeconds()
    {
        // Arrange
        var Sw = new Stopwatch();
        Sw.Start();
        Task.Delay(1500, TestContext.Current.CancellationToken);

        // Act
        var Result = Sw.ElapsedTimeString();

        // Assert
        Assert.Matches(@"^\d{2}:\d{2}$", Result);
    }

    [Fact]
    public async Task ElapsedTimeStringShouldFormatAsMmSs()
    {
        var Sw = new Stopwatch();
        Sw.Start();
        await Task.Delay(1234, TestContext.Current.CancellationToken); // ~1.23 seconds
        Sw.Stop();

        var Result = Sw.ElapsedTimeString();

        // Format should be mm:ss regardless of milliseconds
        Assert.Matches(@"^\d{2}:\d{2}$", Result);
    }

    [Fact]
    public async Task ElapsedTimeStringFormatsAsMmSsWithLeadingZero()
    {
        // Arrange
        var Sw = new Stopwatch();
        Sw.Start();
        await Task.Delay(100, TestContext.Current.CancellationToken);
        Sw.Stop();

        // Act
        var Formatted = Sw.ElapsedTimeString();

        // Assert
        Assert.Matches(@"^00:\d{2}$", Formatted);
    }

    [Fact]
    public async Task ElapsedTimeStringHandlesLargeValues()
    {
        // Arrange
        var Sw = new Stopwatch();
        Sw.Start();
        await Task.Delay(61_000, TestContext.Current.CancellationToken);
        Sw.Stop();

        // Act
        var Formatted = Sw.ElapsedTimeString();

        // Assert
        Assert.Matches(@"^\d{2}:\d{2}$", Formatted);
    }
}
