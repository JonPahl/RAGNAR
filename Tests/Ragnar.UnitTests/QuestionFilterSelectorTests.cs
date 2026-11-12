namespace Ragnar.Tests;

public class QuestionFilterSelectorTests
{
    private readonly Mock<ILogger> _loggerMock = new();

    [Fact]
    public void FindFilterForXmlShouldReturnNonNullFilter()
    {
        // Arrange
        var selector = new QuestionFilterSelector(_loggerMock.Object, []);

        // Act
        var filter = selector.FindFilter(QuestionCategory.XML);

        // Assert
        Assert.NotNull(filter);
    }

    [Fact]
    public void FindFilterForXmlShouldExcludeTestCategoryKeywords()
    {
        // Arrange
        var selector = new QuestionFilterSelector(_loggerMock.Object, []);

        // Act
        var filter = selector.FindFilter(QuestionCategory.XML, 50);

        // Assert
        Assert.NotNull(filter.MustNot);
        Assert.NotEmpty(filter.MustNot);
    }

    [Fact]
    public void FindFilterForUnregisteredCategoryShouldThrow()
    {
        // Arrange
        var selector = new QuestionFilterSelector(_loggerMock.Object, []);

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(
            () => selector.FindFilter(QuestionCategory.Other));

        Assert.Contains("Method", ex.Message);
        Assert.Contains("registered", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FindFilterShouldUseCustomStrategyWhenRegistered()
    {
        // Arrange
        var mockStrategy = new Mock<IFilterStrategy>();
        var expectedFilter = new QdrantFilterBuilder()
            .AddMustMatch("Cat", "XML")
            .Build();
        mockStrategy.Setup(s => s.SupportedCategory).Returns(QuestionCategory.Other);
        mockStrategy.Setup(s => s.CreateFilter(It.IsAny<int>())).Returns(expectedFilter);

        var selector = new QuestionFilterSelector(_loggerMock.Object, [mockStrategy.Object]);

        // Act
        var result = selector.FindFilter(QuestionCategory.Other, 100);

        // Assert
        Assert.Same(expectedFilter, result);
        mockStrategy.Verify(s => s.CreateFilter(100), Times.Once);
    }

    [Fact]
    public void BuiltinXmlStrategyShouldBeRegisteredWhenAbsent()
    {
        // Arrange – pass empty collection so the built-in XML strategy auto-registers
        var selector = new QuestionFilterSelector(_loggerMock.Object, []);

        // Act – should not throw because XML is auto-registered
        var filter = selector.FindFilter(QuestionCategory.XML, 0);

        // Assert
        Assert.NotNull(filter);
    }
}

