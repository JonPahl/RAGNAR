namespace RAGNAR.UnitTests.Util;

public sealed class StopwatchExtensionsTests
{

    [Fact]
    public void ElapsedTimeString_FormatsAsMinutesAndSeconds ()
    {
        // Arrange
        var sw = new Stopwatch();
        sw.Start();
        Task.Delay(1500, TestContext.Current.CancellationToken);

        // Act
        var result = sw.ElapsedTimeString();

        // Assert
        Assert.Matches(@"^\d{2}:\d{2}$", result);
    }

    [Fact]
    public void ElapsedTimeString_ShouldFormatAsMmSs ()
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        Task.Delay(1234, TestContext.Current.CancellationToken); // ~1.23 seconds
        sw.Stop();

        var result = sw.ElapsedTimeString();

        // Format should be mm:ss regardless of milliseconds
        Assert.Matches(@"^\d{2}:\d{2}$", result);
    }

    [Fact]
    public void ElapsedTimeString_FormatsAs_mm_ss_WithLeadingZero ()
    {
        // Arrange
        var sw = new Stopwatch();
        sw.Start();
        Task.Delay(100, TestContext.Current.CancellationToken);
        sw.Stop();

        // Act
        var formatted = sw.ElapsedTimeString();

        // Assert
        Assert.Matches(@"^00:\d{2}$", formatted);
    }

    [Fact]
    public void ElapsedTimeString_HandlesLargeValues ()
    {
        // Arrange
        var sw = new Stopwatch();
        sw.Start();
        Task.Delay(61_000, TestContext.Current.CancellationToken);
        sw.Stop();

        // Act
        var formatted = sw.ElapsedTimeString();

        // Assert
        Assert.Matches(@"^\d{2}:\d{2}$", formatted);
    }
}
