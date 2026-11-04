namespace Ragnar.Tests;

public class QuestionFilterSelectorTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IFilterStrategy> _strategyMock;
    private readonly QuestionFilterSelector _sut;

    public QuestionFilterSelectorTests()
    {
        _loggerMock = new Mock<ILogger>();
        _strategyMock = new Mock<IFilterStrategy>();
        _strategyMock.Setup(s => s.SupportedCategory).Returns(QuestionCategory.XML);
        var filter = new Filter();
        _strategyMock.Setup(s => s.CreateFilter(It.IsAny<int>())).Returns(filter);

        _sut = new QuestionFilterSelector(_loggerMock.Object, [_strategyMock.Object]);
    }

    [Fact]
    public void FindFilterShouldReturnEmptyFilterForUnknownCategory()
    {
        // Act
        var result = _sut.FindFilter(QuestionCategory.XML);

        // Assert
        Assert.NotNull(result);
        // Unknown category should return a default/empty filter
    }

    [Fact]
    public void FindFilterShouldUseDefaultThresholdWhenNotSpecified()
    {
        // Act
        var result = _sut.FindFilter(QuestionCategory.XML);

        // Assert – threshold 0 passed to strategy
        _strategyMock.Verify(s => s.CreateFilter(0), Times.Once);
        Assert.NotNull(result);
    }

    [Fact]
    public void FindFilterShouldLogDebugMessages()
    {
        // Act
        _sut.FindFilter(QuestionCategory.XML, 100);

        // Assert
        _loggerMock.Verify(
            l => l.Debug("It.IsAny<Action<string, string, string>>(), It.IsAny<object?>()", It.IsAny<object?>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public void ConstructorShouldThrowIfStrategiesContainDuplicateCategories()
    {
        // Arrange
        var strategy1 = new Mock<IFilterStrategy>();
        strategy1.Setup(s => s.SupportedCategory).Returns(QuestionCategory.XML);
        var strategy2 = new Mock<IFilterStrategy>();
        strategy2.Setup(s => s.SupportedCategory).Returns(QuestionCategory.XML);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new QuestionFilterSelector(_loggerMock.Object, [strategy1.Object, strategy2.Object]));
    }
}
