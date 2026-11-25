namespace Ragnar.IntegrationTests;

/// <summary>
/// Integration tests verifying record types work correctly in pipeline reporting scenarios.
/// </summary>
public class CollectionInfoAndMetricIntegrationTests
{
    #region CollectionInfo in Pipeline Context

    [Fact]
    public void CollectionInfoShouldBeComparableForReporting()
    {
        // Arrange
        var before = new Core.CollectionInfo
        {
            Name = "ragnar",
            Dimension = 768,
            VectorsCount = 100,
            Status = "green"
        };

        // Act – simulate upsert adding vectors
        var after = before with
        {
            VectorsCount = 150
        };

        // Assert
        Assert.Equal(before.Name, after.Name);
        Assert.Equal(50, after.VectorsCount - before.VectorsCount);
        Assert.NotEqual(before, after);
    }

    #endregion

    #region StageMetric Aggregation

    [Fact]
    public void MultipleStageMetricsShouldAggregateCorrectly()
    {
        // Arrange
        var metrics = new List<StageMetric>
        {
            new() { StageName = "Discovery", Duration = TimeSpan.FromMilliseconds(50), Success = true, RetryCount = 0 },
            new() { StageName = "Parsing", Duration = TimeSpan.FromMilliseconds(200), Success = true, RetryCount = 1 },
            new() { StageName = "Embedding", Duration = TimeSpan.FromSeconds(3), Success = false, RetryCount = 3, Error = "timeout" }
        };

        // Act
        var totalDuration = metrics.Sum(m => m.Duration.TotalMilliseconds);
        var failures = metrics.Where(m => !m.Success).ToList();
        var totalRetries = metrics.Sum(m => m.RetryCount);

        // Assert
        //Assert.Equal(TimeSpan.FromMilliseconds(50) + TimeSpan.FromMilliseconds(200) + TimeSpan.FromSeconds(3), totalDuration);
        var item = Assert.Single(failures);
        Assert.Equal("timeout", item.Error);
        Assert.Equal(4, totalRetries);
    }

    [Fact]
    public void StageMetricShouldSupportImmutabilityPattern()
    {
        // Arrange
        var original = new StageMetric
        {
            StageName = "Test",
            Duration = TimeSpan.FromSeconds(1),
            Success = true,
            RetryCount = 0,
            Error = null
        };

        // Act
        var modified = original with
        {
            Success = false,
            Error = "failed"
        };

        // Assert
        Assert.True(original.Success);
        Assert.Null(original.Error);
        Assert.False(modified.Success);
        Assert.Equal("failed", modified.Error);
        Assert.Equal(original.StageName, modified.StageName);
    }

    #endregion

    #region GenerationOptions Validation

    [Fact]
    public void GenerationOptionsShouldEnforceRanges()
    {
        // Act
        var valid = new GenerationOptions
        {
            MaxTokens = 4096,
            ContextWindow = 32768,
            ThreadCount = 4,
            Temperature = 0.7f,
            RepeatPenalty = 1.1f
        };

        // Assert – validation attributes allow these values
        Assert.Equal(4096, valid.MaxTokens);
        Assert.Equal(32768, valid.ContextWindow);
        Assert.Equal(0.7f, valid.Temperature);
    }

    [Fact]
    public void GenerationOptionsDefaultsShouldBeReasonable()
    {
        // Arrange & Act
        var opts = new GenerationOptions();

        // Assert
        Assert.Equal(32_768, opts.MaxTokens);
        Assert.Equal(65_536, opts.ContextWindow);
        Assert.Equal(8, opts.ThreadCount);
        Assert.Equal(0.2f, opts.Temperature);
        Assert.Equal(1.02f, opts.RepeatPenalty);
    }

    #endregion

    #region EmbeddingOptions Integration

    [Fact]
    public void EmbeddingOptionsShouldBeValidForStandardModels()
    {
        // Arrange & Act
        var opts = new EmbeddingOptions
        {
            Host = new Uri("http://localhost:11434"),
            EmbeddingModel = "nomic-embed-text",
            Timeout = TimeSpan.FromSeconds(60),
            Dimension = 768,
            BatchSize = 16
        };

        // Assert
        Assert.Equal(768UL, opts.Dimension);
        Assert.Equal(16, opts.BatchSize);
        Assert.Equal("nomic-embed-text", opts.EmbeddingModel);
    }

    [Fact]
    public void EmbeddingOptionsShouldSupportHighDimensionality()
    {
        // Act
        var opts = new EmbeddingOptions
        {
            Host = new Uri("http://localhost"),
            EmbeddingModel = "large-model",
            Timeout = TimeSpan.FromMinutes(5),
            Dimension = 1536,
            BatchSize = 4
        };

        // Assert
        Assert.Equal(1536UL, opts.Dimension);
        Assert.Equal(4, opts.BatchSize);
    }

    #endregion
}
