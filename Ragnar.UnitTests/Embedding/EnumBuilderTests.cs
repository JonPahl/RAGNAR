using Ragnar.Plugins;
using Ragnar.Questions;

namespace RAGNAR.UnitTests.Embedding;

public sealed class EnumBuilderTests
{
    [Fact]
    public void All_ReturnsAllQuestionCategories()
    {
        // Act
        var all = LoadQuestionCategories.All();

        // Assert
        Assert.Equal(Enum.GetNames<QuestionCategory>().Length, all.Count);
    }
}
