

namespace Ragnar.Tests;

public class XmlCommentFilterStrategyTests
{
    private readonly Mock<ILogger> _loggerMock = new();
    private readonly XmlCommentFilterStrategy _sut;

    public XmlCommentFilterStrategyTests()
    {
        _sut = new XmlCommentFilterStrategy(_loggerMock.Object);
    }

    [Fact]
    public void SupportedCategoryShouldReturnXml()
    {
        Assert.Equal(QuestionCategory.XML, _sut.SupportedCategory);
    }

    [Fact]
    public void CreateFilterWithZeroThresholdShouldReturnFilterWithRange()
    {
        // Act
        var filter = _sut.CreateFilter(0);

        // Assert
        Assert.NotNull(filter);
        Assert.NotEmpty(filter.Must);
        //Assert.Contains(filter.Must, c => c.MatchKeyword == "CommentLength");
    }

    [Fact]
    public void CreateFilterWithThresholdShouldReturnFilterWithRange()
    {
        // Act
        var filter = _sut.CreateFilter(50);

        // Assert
        Assert.NotNull(filter);
        Assert.NotEmpty(filter.Must);
    }

    [Fact]
    public void CreateFilterShouldExcludeTestRelatedKeywords()
    {
        // Act
        var filter = _sut.CreateFilter(100);

        // Assert – MustNot should contain exclusion conditions
        Assert.NotNull(filter.MustNot);
        Assert.NotEmpty(filter.MustNot);
    }

    [Fact]
    public void CreateFilterShouldLogDebugMessages()
    {
        // Act
        _sut.CreateFilter(42);

        // Assert
        _loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Creating XML comment filter")),
                It.IsAny<Exception?>(), null), Times.AtLeastOnce);

        // It.IsAny<Func<It.IsAnyType, Exception?, string>> ?), Times.AtLeastOnce);
    }
}
