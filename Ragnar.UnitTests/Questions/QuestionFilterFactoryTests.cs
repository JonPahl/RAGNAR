namespace Ragnar.UnitTests;

public class QuestionFilterFactoryTests
{
    [Theory]
    [InlineData(QuestionCategory.XML, 0)]
    [InlineData(QuestionCategory.XML, 100)]
    public void FindFilterXMLCreatesCorrectStrategy(QuestionCategory Category, int Size)
    {
        // Act
        var Filter = QuestionFilterSelector.FindFilter(Category, Size);

        // Assert
        Assert.NotNull(Filter);
        Assert.IsType<Filter>(Filter);
    }

    [Theory]
    [InlineData(QuestionCategory.General)]
    [InlineData(QuestionCategory.Security)]
    public void FindFilterNonXMLReturnsDefaultFilter(QuestionCategory Category)
    {
        // Act
        var Filter = QuestionFilterSelector.FindFilter(Category);

        // Assert
        Assert.IsType<Filter>(Filter);
    }

    [Fact]
    public void FindFilterXMLWithSizeZeroUsesXmlCommentFilterStrategy()
    {
        // Act
        var Filter = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 0);

        // Assert
        Assert.NotNull(Filter);
        Assert.IsType<Filter>(Filter);
    }

    [Fact]
    public void FindFilterXMLWithNonZeroSizeUsesXmlCommentLengthFilterStrategy()
    {
        // Act
        var Filter = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 42);

        // Assert
        Assert.NotNull(Filter);
        Assert.IsType<Filter>(Filter);
    }
}
