// ═══════════════════════════════════════════════════════════
// VectorStoreRepositoryTests.cs
// ═══════════════════════════════════════════════════════════
namespace Ragnar.Tests;

public class XmlCommentFilterStrategyTests
{
    private readonly XmlCommentFilterStrategy _sut = new();

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
        Assert.NotEmpty(filter.Must);
    }

    [Fact]
    public void CreateFilterShouldHaveTwoConditions()
    {
        // Act
        var filter = _sut.CreateFilter(100);

        // Assert
        Assert.Equal(2, filter.Must.Count);
    }

    [Fact]
    public void CreateFilterFirstConditionShouldUseSizeThreshold()
    {
        // Arrange
        const int threshold = 75;

        // Act
        var filter = _sut.CreateFilter(threshold);

        // Assert
        var firstCondition = filter.Must[0];
        Assert.NotNull(firstCondition.Field);
        Assert.Equal("CommentLength", firstCondition.Field.Key);
        Assert.NotNull(firstCondition.Field.Range);
        Assert.Equal(threshold, firstCondition.Field.Range.Lt);
    }

    [Fact]
    public void CreateFilterSecondConditionShouldExcludeTestKeywords()
    {
        // Act
        var filter = _sut.CreateFilter(50);

        // Assert
        var secondCondition = filter.Must[1];
        Assert.Equal("Category", secondCondition.Field.Key);
        var exceptKeywords = secondCondition.Field.Match.ExceptKeywords;
        Assert.NotNull(exceptKeywords);
        Assert.Contains("Tests", exceptKeywords.Strings);
        Assert.Contains("Test", exceptKeywords.Strings);
        Assert.Contains("Testing", exceptKeywords.Strings);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(200)]
    [InlineData(1000)]
    public void CreateFilterDifferentThresholdsShouldSetCorrectLt(int threshold)
    {
        // Act
        var filter = _sut.CreateFilter(threshold);

        // Assert
        Assert.Equal(threshold, filter.Must[0].Field.Range.Lt);
    }
}
