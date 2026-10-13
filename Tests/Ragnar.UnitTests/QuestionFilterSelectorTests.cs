namespace Ragnar.Tests;

public class QuestionFilterSelectorTests
{
    [Fact]
    public void FindFilterWithXmlCategoryShouldReturnNonEmptyFilter()
    {
        IEnumerable<IFilterStrategy> strategies = [];
        // Act
        var x = new QuestionFilterSelector(strategies);
        var filter = x.FindFilter(QuestionCategory.XML);

        // Assert
        Assert.NotNull(filter);
        Assert.False(filter.Must is null || filter.Must.Count == 0);
    }

    [Fact]
    public void FindFilterWithXmlCategoryAndThresholdShouldReturnFilter()
    {
        // Act
        IEnumerable<IFilterStrategy> u = [];
        var x = new QuestionFilterSelector(u);
        var filter = x.FindFilter(QuestionCategory.XML, 100);

        // Assert
        Assert.NotNull(filter);
        // Should have a range condition for CommentLength
        Assert.Contains(filter.Must, c => c.Field?.Key == "CommentLength");
    }

    [Fact]
    public void FindFilterWithUnknownCategoryShouldReturnEmptyFilter()
    {
        // Act
        IEnumerable<IFilterStrategy> u = [];
        var x = new QuestionFilterSelector(u);
        var filter = x.FindFilter(QuestionCategory.Uncategorized, 50);

        // Assert
        Assert.NotNull(filter);
        // Unknown category returns default (empty) filter
    }

    [Fact]
    public void FindFilterWithXmlCategoryShouldExcludeTestCategories()
    {
        // Act
        IEnumerable<IFilterStrategy> u = [];
        var x = new QuestionFilterSelector(u);
        var filter = x.FindFilter(QuestionCategory.XML, 50);

        // Assert
        // Should contain an ExceptKeywords condition for Category
        Assert.Contains(filter.Must, c =>
            c.Field?.Match?.ExceptKeywords?.Strings?.Contains("Test") == true);
    }

    [Fact]
    public void FindFilterWithDefaultThresholdShouldUseZero()
    {
        // Act
        IEnumerable<IFilterStrategy> u = [];
        var x = new QuestionFilterSelector(u);
        var filter = x.FindFilter(QuestionCategory.XML);

        // Assert
        Assert.NotNull(filter);
    }
}
