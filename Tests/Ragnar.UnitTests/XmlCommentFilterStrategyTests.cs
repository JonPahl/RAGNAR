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
        // Act
        var category = _sut.SupportedCategory;

        // Assert
        Assert.Equal(QuestionCategory.XML, category);
    }

    [Fact]
    public void CreateFilterShouldReturnNonEmptyFilter()
    {
        // Act
        var filter = _sut.CreateFilter(100);

        // Assert
        Assert.NotNull(filter);
        Assert.NotEmpty(filter.Must);
    }

    [Fact]
    public void CreateFilterShouldIncludeCommentLengthRange()
    {
        // Act
        var filter = _sut.CreateFilter(50);

        // Assert
        Assert.Contains(filter.Must, c => c.Field?.Key == "CommentLength");
    }

    [Fact]
    public void CreateFilterShouldExcludeTestCategories()
    {
        // Act
        var filter = _sut.CreateFilter(50);

        // Assert
        var hasExceptKeywords = filter.Must.Any(c =>
            c.Field?.Match?.ExceptKeywords?.Strings != null &&
            c.Field.Match.ExceptKeywords.Strings.Contains("Test"));
        Assert.True(hasExceptKeywords);
    }

    [Fact]
    public void CreateFilterShouldExcludeTestsCategory()
    {
        // Act
        var filter = _sut.CreateFilter(50);

        // Assert
        var hasExceptKeywords = filter.Must.Any(c =>
            c.Field?.Match?.ExceptKeywords?.Strings != null &&
            c.Field.Match.ExceptKeywords.Strings.Contains("Tests"));
        Assert.True(hasExceptKeywords);
    }

    [Fact]
    public void CreateFilterShouldExcludeTestingCategory()
    {
        // Act
        var filter = _sut.CreateFilter(50);

        // Assert
        var hasExceptKeywords = filter.Must.Any(c =>
            c.Field?.Match?.ExceptKeywords?.Strings != null &&
            c.Field.Match.ExceptKeywords.Strings.Contains("Testing"));
        Assert.True(hasExceptKeywords);
    }

    [Fact]
    public void CreateFilterWithDifferentThresholdsShouldProduceDifferentFilters()
    {
        // Act
        var filterA = _sut.CreateFilter(50);
        var filterB = _sut.CreateFilter(200);

        // Assert – filters are distinct objects
        Assert.NotSame(filterA, filterB);
    }
}
