using Ragnar.Plugins;
using Ragnar.Questions;

namespace RAGNAR.UnitTests.Util;

public sealed class LoadEnumsTests
{
    [Fact]
    public void All_ReturnsAllQuestionCategories()
    {
        // Act
        var all = LoadQuestionCategories.All();

        // Assert
        Assert.Equal(Enum.GetNames<QuestionCategory>().Length, all.Count);
        Assert.Contains(QuestionCategory.Refactor, all);
        Assert.Contains(QuestionCategory.Security, all);
    }
}
