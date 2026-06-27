using Ragnar.Extensions;

using System.Diagnostics;

namespace RAGNAR.UnitTests.Util;

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
        var formatted = sw.ElapsedTimeString();

        // Assert
        Assert.Matches(@"^00:\d{2}$", formatted);
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
        var formatted = sw.ElapsedTimeString();

        // Assert
        Assert.Matches(@"^\d{2}:\d{2}$", formatted);
    }
}
