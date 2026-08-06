namespace Ragnar.IntegrationTests;

public class QuestionFilterSelectorTests
{
    [Theory]
    [InlineData(QuestionCategory.XML, 0)]
    [InlineData(QuestionCategory.XML, 100)]
    public void FindFilter_ShouldReturnValidFilter_ForXmlCategory(QuestionCategory category, int size)
    {
        // Act
        var filter = QuestionFilterSelector.FindFilter(category, size);

        // Assert
        filter.Should().NotBeNull();
        // Optional: assert structure based on strategy (e.g., length-based vs empty)
    }

    [Fact]
    public void FindFilter_ShouldReturnEmptyFilter_ForUnknownCategory()
    {
        // Act
        var filter = QuestionFilterSelector.FindFilter((QuestionCategory)999);

        // Assert
        filter.Should().NotBeNull();
        filter.Should.Should().BeEmpty();
    }
}
