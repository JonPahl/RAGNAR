namespace Ragnar.Tests;

public class XmlCommentFilterStrategyTests
{
    [Fact]
    public void SupportedCategoryShouldBeXml()
    {
        var logger = new Mock<Serilog.ILogger>();
        var strategy = new XmlCommentFilterStrategy(logger.Object);

        Assert.Equal(QuestionCategory.XML, strategy.SupportedCategory);
    }

    [Fact]
    public void CreateFilterShouldReturnNonEmptyFilter()
    {
        var logger = new Mock<Serilog.ILogger>();
        var strategy = new XmlCommentFilterStrategy(logger.Object);

        var filter = strategy.CreateFilter(50);

        Assert.NotNull(filter);
    }

    [Fact]
    public void CreateFilterWithZeroThresholdShouldStillWork()
    {
        var logger = new Mock<Serilog.ILogger>();
        var strategy = new XmlCommentFilterStrategy(logger.Object);

        var filter = strategy.CreateFilter(0);

        Assert.NotNull(filter);
    }

    [Fact]
    public void CreateFilterWithLargeThresholdShouldStillWork()
    {
        var logger = new Mock<Serilog.ILogger>();
        var strategy = new XmlCommentFilterStrategy(logger.Object);

        var filter = strategy.CreateFilter(10000);

        Assert.NotNull(filter);
    }
}
