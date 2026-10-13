namespace Ragnar.Tests;

public class QuestionCombineBuilderExtensionsTests
{
    [Fact]
    public void SetActiveTrueShouldCallAsActive()
    {
        // Arrange
        var builder = new QuestionBuilder();

        // Act
        var result = builder.SetActive(true);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void SetActiveFalseShouldCallAsInactive()
    {
        // Arrange
        var builder = new QuestionBuilder();

        // Act
        var result = builder.SetActive(false);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void SetActiveShouldReturnSameBuilderForChaining()
    {
        // Arrange
        var builder = new QuestionBuilder();

        // Act
        var result = builder.SetActive(true).SetActive(false);

        // Assert
        Assert.Same(builder, result);
    }
}
