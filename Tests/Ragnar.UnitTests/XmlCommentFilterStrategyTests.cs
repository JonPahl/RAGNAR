namespace Ragnar.Tests;

public class XmlCommentFilterStrategyTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly XmlCommentFilterStrategy _sut;

    public XmlCommentFilterStrategyTests()
    {
        _loggerMock = new Mock<ILogger>();
        _sut = new XmlCommentFilterStrategy(_loggerMock.Object);
    }

    [Fact]
    public void SupportedCategoryShouldReturnXml()
    {
        // Act
        var category = _sut.SupportedCategory;

        // Assert
        Assert.Equal(QuestionCategory.XML, category);
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
    public void CreateFilterShouldContainRangeCondition()
    {
        // Act
        var filter = _sut.CreateFilter(100);

        // Assert
        Assert.NotNull(filter.Must);
        Assert.True(filter.Must.Count > 0);
    }

    [Fact]
    public void CreateFilterShouldContainExcludedKeywords()
    {
        // Act
        var filter = _sut.CreateFilter(50);

        // Assert – MustNot should contain excluded categories
        Assert.NotNull(filter.MustNot);
        Assert.True(filter.MustNot.Count > 0);
    }

    [Fact]
    public void CreateFilterShouldLogDebugMessages()
    {
        // Act
        _sut.CreateFilter(75);

        // Assert
        _loggerMock.Verify(
            l => l.Debug("It.IsAny<Action<string, string>>()", It.IsAny<object?>()),
            Times.AtLeastOnce);
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
    public void CreateFilterShouldBeConsistentForSameThreshold()
    {
        // Act
        var f1 = _sut.CreateFilter(100);
        var f2 = _sut.CreateFilter(100);

        // Assert
        Assert.Equal(f1.Must?.Count, f2.Must?.Count);
        Assert.Equal(f1.MustNot?.Count, f2.MustNot?.Count);
    }
}
