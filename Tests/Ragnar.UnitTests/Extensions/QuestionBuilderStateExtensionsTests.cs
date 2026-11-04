namespace Ragnar.Tests.Extensions;

public class QuestionBuilderStateExtensionsTests
{
    [Fact]
    public void SetActiveTrueShouldCallAsActive()
    {
        // Act
        var builder = new QuestionBuilder();
        var result = builder.SetActive(true);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void SetActiveFalseShouldCallAsInactive()
    {
        // Act
        var builder = new QuestionBuilder();
        var result = builder.SetActive(false);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void SetActiveShouldReturnBuilderForChaining()
    {
        var builder = new QuestionBuilder();
        var result = builder.SetActive(true).SetActive(false);

        Assert.NotNull(result);
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

