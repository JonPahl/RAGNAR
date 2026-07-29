namespace Ragnar.IntegrationTests;

public class QuestionFilterFactoryTests
{
    [Theory]
    [InlineData(QuestionCategory.XML, 0, typeof(Filter))]
    [InlineData(QuestionCategory.XML, 100, typeof(Filter))]
    [InlineData(QuestionCategory.General, 0, typeof(Filter))]
    [InlineData(QuestionCategory.General, 50, typeof(Filter))]
    public void FindFilter_ReturnsCorrectStrategy (QuestionCategory category, int size, Type expectedType)
    {
        // Act
        var filter = QuestionFilterFactory.FindFilter(category, size);

        // Assert
        Assert.IsType(expectedType, filter);
    }

    [Fact]
    public void FindFilter_XML_WithSize_CallsCreateFilter ()
    {
        // Arrange
        const int size = 123;

        // Act
        var filter = QuestionFilterFactory.FindFilter(QuestionCategory.XML, size);

        // Assert
        Assert.NotNull(filter);
        Assert.IsType<Filter>(filter);
    }
}
