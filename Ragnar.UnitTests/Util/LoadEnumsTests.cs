namespace Ragnar.Tests.Util;

public sealed class LoadEnumsTests
{
    [Fact]
    public void All_ReturnsAllQuestionCategories()
    {
        // Act
        var All = LoadQuestionCategories.All();

        // Assert
        Assert.Equal(Enum.GetNames<QuestionCategory>().Length, All.Count);
        All.Should().Contain(QuestionCategory.Refactor);
        All.Should().Contain(QuestionCategory.Security);
    }
}
