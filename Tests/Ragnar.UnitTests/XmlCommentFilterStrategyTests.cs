namespace Ragnar.Tests;

public class XmlCommentFilterStrategyTests
{
    private readonly XmlCommentFilterStrategy _sut;

    public XmlCommentFilterStrategyTests()
    {
        _sut = new XmlCommentFilterStrategy();
    }

    [Fact]
    public void SupportedCategoryShouldReturnXml()
    {
        Assert.Equal(QuestionCategory.XML, _sut.SupportedCategory);
    }

    [Fact]
    public void CreateFilterWithZeroThresholdShouldReturnFilterWithRangeAndExcept()
    {
        // Act
        var filter = _sut.CreateFilter(0);

        // Assert
        Assert.NotNull(filter);
        Assert.NotEmpty(filter.Must);
    }

    [Fact]
    public void CreateFilterWithPositiveThresholdShouldReturnFilterWithRange()
    {
        // Act
        var filter = _sut.CreateFilter(100);

        // Assert
        Assert.NotNull(filter);
        Assert.NotEmpty(filter.Must);
        // Should contain a range condition for CommentLength
        var rangeCondition = filter.Must.FirstOrDefault(c => c.Field?.Key == "CommentLength");
        Assert.NotNull(rangeCondition);
    }

    [Fact]
    public void CreateFilterShouldExcludeTestCategoryKeywords()
    {
        // Act
        var filter = _sut.CreateFilter(50);

        // Assert
        Assert.NotNull(filter);
        // Should have an except-keywords condition for Category
        var exceptCondition = filter.Must.FirstOrDefault(c => c.Field?.Key == "Category");
        Assert.NotNull(exceptCondition);
        Assert.NotNull(exceptCondition.Field?.Match?.ExceptKeywords);
        Assert.Contains("Test", exceptCondition.Field.Match.ExceptKeywords.Strings);
        Assert.Contains("Tests", exceptCondition.Field.Match.ExceptKeywords.Strings);
        Assert.Contains("Testing", exceptCondition.Field.Match.ExceptKeywords.Strings);
    }

    [Fact]
    public void CreateFilterShouldBeDeterministicForSameThreshold()
    {
        // Act
        var filter1 = _sut.CreateFilter(25);
        var filter2 = _sut.CreateFilter(25);

        // Assert
        Assert.Equal(filter1.Must.Count, filter2.Must.Count);
    }
}
