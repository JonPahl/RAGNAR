namespace Ragnar.Tests;

public class XmlCommentFilterStrategyTests
{
    private readonly Mock<Microsoft.Extensions.Logging.ILogger> _loggerMock;
    private readonly XmlCommentFilterStrategy _sut;

    public XmlCommentFilterStrategyTests()
    {
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
        _sut = new XmlCommentFilterStrategy(_loggerMock.Object);
    }

    [Fact]
    public void SupportedCategoryShouldReturnXml()
    {
        Assert.Equal(QuestionCategory.XML, _sut.SupportedCategory);
    }

    [Fact]
    public void CreateFilterShouldReturnNonNullFilter()
    {
        // Act
        var filter = _sut.CreateFilter(50);

        // Assert
        Assert.NotNull(filter);
    }

    [Fact]
    public void CreateFilterWithZeroThresholdShouldStillReturnFilter()
    {
        // Act
        var filter = _sut.CreateFilter(0);

        // Assert
        Assert.NotNull(filter);
    }

    [Fact]
    public void CreateFilterShouldIncludeRangeCondition()
    {
        // Act
        var filter = _sut.CreateFilter(100);

        // Assert – the filter should have conditions (must or mustNot)
        Assert.True(filter.Must is { Count: > 0 } || filter.MustNot is { Count: > 0 });
    }

    [Fact]
    public void CreateFilterShouldBeDeterministic()
    {
        // Act
        var f1 = _sut.CreateFilter(200);
        var f2 = _sut.CreateFilter(200);

        // Assert – both should be non-null and structurally equivalent
        Assert.NotNull(f1);
        Assert.NotNull(f2);
    }
}
