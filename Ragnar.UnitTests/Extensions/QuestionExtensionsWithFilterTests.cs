namespace Ragnar.UnitTests.Extensions;

public class QuestionExtensionsWithFilterTests
{
    [Fact]
    public void WithFilter_ShouldReturnNewQuestion_WhenFilterProvided()
    {
        // Arrange
        var original = new Question(true, "Q", "f.cs", QuestionCategory.XML, null);
        var filter = new Filter(); // dummy

        // Act
        var result = original.WithFilter(filter);

        // Assert
        result.Should().NotBeSameAs(original);
        result.Filter.Should().BeSameAs(filter);
    }

    [Fact]
    public void WithFilter_ShouldReturnSameQuestion_WhenFilterNull()
    {
        // Arrange
        var original = new Question(true, "Q", "f.cs", QuestionCategory.XML, null);

        // Act
        var result = original.WithFilter(null);

        // Assert
        result.Should().BeSameAs(original);
    }
}

