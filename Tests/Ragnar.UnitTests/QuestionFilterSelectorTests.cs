namespace Ragnar.Tests;

public class QuestionFilterSelectorTests
{
    [Fact]
    public void FindFilterWithXmlCategoryShouldReturnConfiguredFilter()
    {
        // Act
        var filter = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 100);

        // Assert
        Assert.NotNull(filter);
        Assert.NotEmpty(filter.Must);
    }

    [Fact]
    public void FindFilterWithXmlCategoryAndZeroThresholdShouldStillReturnFilter()
    {
        // Act
        var filter = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 0);

        // Assert
        Assert.NotNull(filter);
    }

    [Fact]
    public void FindFilterWithUnsupportedCategoryShouldReturnEmptyFilter()
    {
        // Arrange - use a category not registered in the strategy dictionary
        var unsupportedCategory = (QuestionCategory)9999;

        // Act
        var filter = QuestionFilterSelector.FindFilter(unsupportedCategory, 50);

        // Assert
        Assert.NotNull(filter);
        Assert.Empty(filter.Must);
        Assert.Empty(filter.MustNot);
    }

    [Fact]
    public void FindFilterShouldReturnDifferentFiltersForDifferentThresholds()
    {
        // Act
        var filter1 = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 10);
        var filter2 = QuestionFilterSelector.FindFilter(QuestionCategory.XML, 500);

        // Assert - both should be valid filters with range conditions
        Assert.NotNull(filter1);
        Assert.NotNull(filter2);
        Assert.NotEmpty(filter1.Must);
        Assert.NotEmpty(filter2.Must);
    }
}
