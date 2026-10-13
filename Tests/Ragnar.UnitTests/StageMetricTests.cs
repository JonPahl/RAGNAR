namespace Ragnar.Tests;

public class StageMetricTests
{
    [Fact]
    public void StageMetricShouldStoreAllProperties()
    {
        var duration = TimeSpan.FromMilliseconds(123);
        var metric = new StageMetric()
        {
            StageName = "Parsing",
            Duration = duration,
            Success = true,
            RetryCount = 2,
            Error = null
        };

        Assert.Equal("Parsing", metric.StageName);
        Assert.Equal(duration, metric.Duration);
        Assert.True(metric.Success);
        Assert.Equal(2, metric.RetryCount);
        Assert.Null(metric.Error);
    }

    [Fact]
    public void StageMetricWithFailureShouldStoreError()
    {
        var metric = new StageMetric()
        {
            StageName = "Parsing",
            Duration = TimeSpan.Zero,
            Success = false,
            RetryCount = 3,
            Error = "Connection refused"
        };

        Assert.False(metric.Success);
        Assert.Equal("Connection refused", metric.Error);
        Assert.Equal(3, metric.RetryCount);
    }

    [Fact]
    public void StageMetricShouldSupportEquality()
    {
        var m1 = new StageMetric()
        {
            StageName = "A",
            Duration = TimeSpan.FromSeconds(1),
            Success = true,
            RetryCount = 0,
            Error = null
        };
        var m2 = new StageMetric()
        {
            StageName = "A",
            Duration = TimeSpan.FromSeconds(1),
            Success = true,
            RetryCount = 0,
            Error = null
        };

        Assert.Equal(m1, m2);
    }

    [Fact]
    public void StageMetricShouldDifferWhenErrorDiffers()
    {
        var m1 = new StageMetric()
        {
            StageName = "A",
            Duration = TimeSpan.Zero,
            Success = false,
            RetryCount = 0,
            Error = "err1"
        };
        var m2 = new StageMetric()
        {
            StageName = "A",
            Duration = TimeSpan.Zero,
            Success = false,
            RetryCount = 0,
            Error = "err2"
        };

        Assert.NotEqual(m1, m2);
    }
}
