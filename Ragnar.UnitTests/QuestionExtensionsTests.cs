namespace RAGNAR.UnitTests;

public class QuestionExtensionsTests
{
    [Fact]
    public void MatchesCategories_ShouldThrow_WhenCategoriesNull()
    {
        // Arrange
        var questions = new List<Question>().ToImmutableList();

        // Act & Assert
        Action act = () => questions.MatchesCategories(null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(new[] { QuestionCategory.XML }, 2)]
    [InlineData(new[] { QuestionCategory.XML, QuestionCategory.Other }, 3)]
    public void MatchesCategories_ShouldFilterByCategory(QuestionCategory[] Categories, int ExpectedCount)
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML, null),
            new(true, "Q2", "f2.cs", QuestionCategory.XML, null),
            new(true, "Q3", "f3.cs", QuestionCategory.Other, null)
        }.ToImmutableList();

        var catSet = Categories.ToImmutableHashSet();

        // Act
        var filtered = Questions.MatchesCategories(catSet);

        // Assert
        filtered.Should().HaveCount(ExpectedCount);
    }
}
