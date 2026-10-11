namespace Ragnar.Tests;

public class StageMetricTests
{
    [Fact]
    public void StageMetricShouldStoreAllProperties()
    {
        var duration = TimeSpan.FromMilliseconds(123);
        var metric = new StageMetric("Parsing", duration, true, 2, null);

        Assert.Equal("Parsing", metric.StageName);
        Assert.Equal(duration, metric.Duration);
        Assert.True(metric.Success);
        Assert.Equal(2, metric.RetryCount);
        Assert.Null(metric.Error);
    }

    [Fact]
    public void StageMetricWithFailureShouldStoreError()
    {
        var metric = new StageMetric("Parsing", TimeSpan.Zero, false, 3, "Connection refused");

        Assert.False(metric.Success);
        Assert.Equal("Connection refused", metric.Error);
        Assert.Equal(3, metric.RetryCount);
    }

    [Fact]
    public void StageMetricShouldSupportEquality()
    {
        var m1 = new StageMetric("A", TimeSpan.FromSeconds(1), true, 0, null);
        var m2 = new StageMetric("A", TimeSpan.FromSeconds(1), true, 0, null);

        Assert.Equal(m1, m2);
    }

    [Fact]
    public void StageMetricShouldDifferWhenErrorDiffers()
    {
        var m1 = new StageMetric("A", TimeSpan.Zero, false, 0, "err1");
        var m2 = new StageMetric("A", TimeSpan.Zero, false, 0, "err2");

        Assert.NotEqual(m1, m2);
    }
}
