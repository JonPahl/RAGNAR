
namespace RAGNAR.UnitTests;

public class QuestionFilterSelectorTests
{
    [Theory]
    [InlineData(QuestionCategory.XML, 0)]
    [InlineData(QuestionCategory.XML, 100)]
    public void FindFilter_ShouldReturnCorrectFilter_ForXmlCategory(QuestionCategory category, int size)
    {
        // Act
        var filter = QuestionFilterSelector.FindFilter(category, size);

        // Assert
        filter.Should().NotBeNull();
        // You can further assert on filter structure depending on your filter strategies
    }

    [Fact]
    public void FindFilter_ShouldReturnEmptyFilter_ForUnknownCategory()
    {
        // Act
        var filter = QuestionFilterSelector.FindFilter((QuestionCategory)999);

        // Assert
        filter.Should().NotBeNull();
        filter.Should.Should().BeEmpty(); // assuming empty Filter has no Should/Filter clauses
    }
}
