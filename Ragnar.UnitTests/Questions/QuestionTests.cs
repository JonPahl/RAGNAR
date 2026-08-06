namespace RAGNAR.UnitTests.Questions;

public sealed class QuestionTests
{
    [Fact]
    public void IsActiveCreatesEnabledQuestion()
    {
        // Act
        var Question = Ragnar.Core.Model.Question.IsActive("Test?", "test", QuestionCategory.Refactor);

        // Assert
        Assert.True(Question.IsEnabled);
        Assert.Equal("Test?", Question.Text);
        Assert.Equal("test", Question.Filename);
        Assert.Equal(QuestionCategory.Refactor, Question.Category);
    }

    [Fact]
    public void IsDisabledCreatesDisabledQuestion()
    {
        // Act
        var Question = Ragnar.Core.Model.Question.IsDisabled("Disabled?", "disabled", QuestionCategory.Logging);

        // Assert
        Assert.False(Question.IsEnabled);
        Assert.Equal("Disabled?", Question.Text);
        Assert.Equal("disabled", Question.Filename);
        Assert.Equal(QuestionCategory.Logging, Question.Category);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsActiveThrowsWhenTextIsInvalid(string? Text)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Question.IsActive(Text!, "key", QuestionCategory.Refactor));
    }

    [Fact]
    public void FromFilePathAndIndexGeneratesDeterministicId()
    {
        // Arrange
        const string Path = "test.cs";
        const string Index = "5";

        // Act
        var Id1 = Point.FromFilePathAndIndex(Path.AsSpan(), Index.AsSpan());
        var Id2 = Point.FromFilePathAndIndex(Path.AsSpan(), Index.AsSpan());

        // Assert
        Assert.Equal(Id1, Id2);
        Assert.NotEqual(0UL, Id1);
    }

    [Fact]
    public void FromFilePathAndIndexHandlesLongPaths()
    {
        // Arrange
        const string Path = "very/long/path/to/a/file/with/many/directories/Program.cs";
        const string Index = "42";

        // Act
        var Id = Point.FromFilePathAndIndex(Path.AsSpan(), Index.AsSpan());

        // Assert
        Assert.NotEqual(0UL, Id);
    }
}
