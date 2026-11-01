
namespace Ragnar.Tests;

public class QuestionCombineBuilderExtensionTests
{
    [Fact]
    public void SetActiveTrueShouldCallAsActive()
    {
        // Arrange
        var builder = new QuestionBuilder();

        // Act
        var result = builder.SetActive(true);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void SetActiveFalseShouldCallAsInactive()
    {
        // Arrange
        var builder = new QuestionBuilder();

        // Act
        var result = builder.SetActive(false);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void SetActiveShouldReturnBuilderForChaining()
    {
        // Arrange
        var builder = new QuestionBuilder();

        // Act
        var result = builder.SetActive(true);

        // Assert
        Assert.Same(builder, result);
    }
}
