namespace Ragnar.IntegrationTests;

public class QuestionFilterFactoryTests
{
    [Theory]
    [InlineData(QuestionCategory.XML, 0, typeof(Filter))]
    [InlineData(QuestionCategory.XML, 100, typeof(Filter))]
    [InlineData(QuestionCategory.Other, 0, typeof(Filter))]
    [InlineData(QuestionCategory.General, 50, typeof(Filter))]
    public void FindFilter_ReturnsCorrectFilterType(QuestionCategory Category, int Size, Type ExpectedType)
    {
        // Act
        var Filter = QuestionFilterSelector.FindFilter(Category, Size);

        // Assert
        Assert.IsType(ExpectedType, Filter);
    }

    [Fact]
    public void FindFilterXMLWithSizeCreatesLengthFilter()
    {
        // Act
        var Filter = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 42);

        // Assert
        Assert.IsType<Filter>(Filter);
    }

    [Fact]
    public void FindFilter_XML_WithoutSize_CreatesBasicFilter()
    {
        // Act
        var Filter = QuestionFilterSelector.FindFilter(QuestionCategory.XML);

        // Assert
        Assert.IsType<Filter>(Filter);
    }
}
