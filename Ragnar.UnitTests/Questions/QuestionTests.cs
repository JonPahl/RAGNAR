namespace Ragnar.Tests.Questions;

public sealed class QuestionTests
{
    [Fact]
    public void IsActive_CreatesEnabledQuestion()
    {
        // Act
        var Q = Question.IsActive("Test?", "test", QuestionCategory.Refactor);

        // Assert
        Q.IsEnabled.Should().BeTrue();
        Q.Text.Should().Be("Test?");
        Q.Filename.Should().Be("test");
        Q.Category.Should().Be(QuestionCategory.Refactor);
    }

    [Fact]
    public void IsDisabled_CreatesDisabledQuestion()
    {
        // Act
        var question = Question.IsDisabled("Disabled?", "disabled", QuestionCategory.Logging);

        // Assert
        question.IsEnabled.Should().BeFalse();
        question.Text.Should().Be("Disabled?");
        question.Filename.Should().Be("disabled");
        question.Category.Should().Be(QuestionCategory.Logging);
    }

    [Fact]
    public void FromFilePathAndIndex_GeneratesDeterministicId()
    {
        // Arrange
        const string Path = "test.cs";
        const string Index = "5";

        // Act
        var Id1 = Point.FromFilePathAndIndex(Path.AsSpan(), Index.AsSpan());
        var Id2 = Point.FromFilePathAndIndex(Path.AsSpan(), Index.AsSpan());

        // Assert
        Id1.Should().Be(Id2);
        Id1.Should().NotBe(0UL);
    }
}
