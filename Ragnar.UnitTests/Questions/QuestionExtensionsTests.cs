namespace Ragnar.UnitTests.Questions;

public class QuestionExtensionsTests
{
    [Fact]
    public void ActiveOnly_ShouldReturnOnlyEnabledQuestions()
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML),
            new(false, "Q2", "f2.cs", QuestionCategory.XML),
            new(true, "Q3", "f3.cs", QuestionCategory.General)
        };

        // Act
        var Active = Questions.ActiveOnly();

        // Assert
        Active.Should().HaveCount(2);
        Active.Should().Contain(Q => Q.Text == "Q1");
        Active.Should().Contain(Q => Q.Text == "Q3");
    }

    [Fact]
    public void InActiveOnly_ShouldReturnOnlyDisabledQuestions()
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML),
            new(false, "Q2", "f2.cs", QuestionCategory.XML),
            new(true, "Q3", "f3.cs", QuestionCategory.General)
        };

        // Act
        var Inactive = Questions.InActiveOnly();

        // Assert
        Inactive.Should().HaveCount(1);
        Inactive.Should().Contain(Q => Q.Text == "Q2");
    }

    [Fact]
    public void MatchesCategories_ThrowsWhenCategoriesNull()
    {
        // Arrange
        var Questions = new List<Question>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Questions.MatchesCategories(null));
    }

    [Theory]
    [InlineData(new QuestionCategory[] { QuestionCategory.XML, QuestionCategory.Other, QuestionCategory.General }, 3)]
    [InlineData(new[] { QuestionCategory.XML }, 2)]
    public void MatchesCategories_ReturnsFilteredQuestions(QuestionCategory[]? Categories, int ExpectedCount)
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML),
            new(false, "Q2", "f2.cs", QuestionCategory.XML),
            new(true, "Q3", "f3.cs", QuestionCategory.Other)
        };
        var Set = Categories is null or { Length: 0 } ? null : ImmutableHashSet.Create(Categories);

        // Act
        var Result = Questions.MatchesCategories(Set);

        // Assert
        Result.Should().HaveCount(ExpectedCount);
    }

    [Fact]
    public void WithFilter_ApplyFilter_WhenFilterNotNull()
    {
        // Arrange
        var Question = new Question(true, "Q1", "f1.cs", QuestionCategory.XML);
        var Filter = new Filter();

        // Act
        var Result = Question.WithFilter(Filter);

        // Assert
        Result.Should().NotBeSameAs(Question);
        Result.Filter.Should().BeSameAs(Filter);
    }

    [Fact]
    public void WithFilter_ReturnsOriginal_WhenFilterNull()
    {
        // Arrange
        var Question = new Question(true, "Q1", "f1.cs", QuestionCategory.XML);

        // Act
        var Result = Question.WithFilter(null);

        // Assert
        Result.Should().BeSameAs(Question);
    }

    [Fact]
    public void ValidateQuestion_ThrowsWhenTextWhitespace()
    {
        // Arrange
        var Question = new Question(true, "   ", "f1.cs", QuestionCategory.XML);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Question.ValidateQuestion());
    }

    [Fact]
    public void ValidateQuestion_ThrowsWhenFilenameWhitespace()
    {
        // Arrange
        var Question = new Question(true, "Q1", "   ", QuestionCategory.XML);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Question.ValidateQuestion());
    }

    [Fact]
    public void ValidateQuestion_ReturnsSameQuestion_WhenValid()
    {
        // Arrange
        var Question = new Question(true, "Q1", "f1.cs", QuestionCategory.XML);

        // Act
        var Result = Question.ValidateQuestion();

        // Assert
        Result.Should().BeSameAs(Question);
    }

    [Fact]
    public void ToActiveOrDisabledQuestion_Enabled_ReturnsActive()
    {
        // Arrange
        var Question = new Question(true, "Q1", "f1.cs", QuestionCategory.XML);

        // Act
        var Result = Question.ToActiveOrDisabledQuestion();

        // Assert
        Result.IsEnabled.Should().BeTrue();
        Result.Text.Should().Be("Q1");
    }

    [Fact]
    public void ToActiveOrDisabledQuestion_Disabled_ReturnsDisabled()
    {
        // Arrange
        var Question = new Question(false, "Q2", "f2.cs", QuestionCategory.XML);

        // Act
        var Result = Question.ToActiveOrDisabledQuestion();

        // Assert
        Result.IsEnabled.Should().BeFalse();
        Result.Text.Should().Be("Q2");
    }

    [Fact]
    public void MatchesCategories_ShouldThrow_WhenCategoriesNull()
    {
        // Arrange
        var Questions = new List<Question>().ToImmutableList();

        // Act & Assert
        Action Act = () => Questions.MatchesCategories(null);
        Act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(new[] { QuestionCategory.XML }, 2)]
    [InlineData(new[] { QuestionCategory.XML, QuestionCategory.Other }, 3)]
    //[InlineData(Array.Empty<QuestionCategory>(), 3)]
    public void MatchesCategories_ShouldFilterByCategory(QuestionCategory[] Categories, int ExpectedCount)
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML, null),
            new(true, "Q2", "f2.cs", QuestionCategory.XML, null),
            new(true, "Q3", "f3.cs", QuestionCategory.Other, null)
        }.ToImmutableList();

        var CatSet = Categories.ToImmutableHashSet();

        // Act
        var Filtered = Questions.MatchesCategories(CatSet);

        // Assert
        Filtered.Should().HaveCount(ExpectedCount);
    }

    [Theory]
    [InlineData("", "file.cs", "Text cannot be null/whitespace")]
    [InlineData("  ", "file.cs", "Text cannot be null/whitespace")]
    [InlineData("What does this do?", "", "Filename cannot be null/whitespace")]
    public void ValidateQuestion_ShouldThrow_WhenInvalid(string text, string filename, string expectedMessage)
    {
        // Arrange
        var Question = new Question(
            IsEnabled: true,
            Text: text,
            Filename: filename,
            Category: QuestionCategory.XML,
            Filter: null);

        // Act
        Action Act = () => Question.ValidateQuestion();

        // Assert
        Act.Should().Throw<ArgumentException>()
           .WithMessage($"*{expectedMessage}*");
    }

    [Fact]
    public void ValidateQuestion_ShouldReturnSameQuestion_WhenValid()
    {
        // Arrange
        var Question = new Question(
            IsEnabled: true,
            Text: "What does this do?",
            Filename: "Program.cs",
            Category: QuestionCategory.XML,
            Filter: null);

        // Act
        var result = Question.ValidateQuestion();

        // Assert
        result.Should().BeSameAs(Question);
    }


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
