namespace RAGNAR.UnitTests.Util;

public sealed class LoadEnumTests
{
    [Fact]
    public void AllReturnsAllQuestionCategories()
    {
        // Act
        var All = LoadQuestionCategories.All();

        // Assert
        Assert.Equal(System.Enum.GetNames<QuestionCategory>().Length, All.Count);
        Assert.Contains(QuestionCategory.Refactor, All);
        Assert.Contains(QuestionCategory.Security, All);
    }
}
