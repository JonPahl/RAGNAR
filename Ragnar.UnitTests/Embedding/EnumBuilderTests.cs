namespace RAGNAR.UnitTests.Embedding;

public sealed class EnumBuilderTests
{
    [Fact]
    public void AllReturnsAllQuestionCategories()
    {
        // Act
        var all = LoadQuestionCategories.All();

        // Assert
        Assert.Equal(System.Enum.GetNames<QuestionCategory>().Length, all.Count);
    }
}
