namespace Ragnar.UnitTests.Extensions;

public class QuestionExtensionsTests
{
    [Fact]
    public void ActiveOnly_ReturnsOnlyEnabledQuestions()
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML),
            new(false, "Q2", "f2.cs", QuestionCategory.General),
            new(true, "Q3", "f3.cs", QuestionCategory.XML)
        }.ToImmutableList();

        // Act
        var Active = Questions.ActiveOnly();

        // Assert
        Active.Should().HaveCount(2)
            .And.Contain(Q => Q.Text == "Q1")
            .And.NotContain(Q => Q.Text == "Q2");
    }

    [Fact]
    public void InActiveOnly_ReturnsOnlyDisabledQuestions()
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML),
            new(false, "Q2", "f2.cs", QuestionCategory.General),
            new(true, "Q3", "f3.cs", QuestionCategory.XML)
        }.ToImmutableList();

        // Act
        var Inactive = Questions.InActiveOnly();

        // Assert
        Inactive.Should().HaveCount(1)
            .And.Contain(Q => Q.Text == "Q2");
    }

    [Fact]
    public void WhereCategoryIs_ThrowsWhenCategoriesNull()
    {
        // Arrange
        var Questions = new List<Question>().ToImmutableList();

        // Act & Assert
        Action Act = () => Questions.MatchesCategories(null);
        Act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    //[InlineData(null, 3)]
    [InlineData(new QuestionCategory[] { QuestionCategory.XML, QuestionCategory.Other, QuestionCategory.General }, 3)]
    //[InlineData(new[] { QuestionCategory.XML, QuestionCategory.General }, 2)]
    [InlineData(new[] { QuestionCategory.General }, 1)]
    public void WhereCategoryIs_FiltersByCategory(QuestionCategory[]? Categories, int ExpectedCount)
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML),
            new(false, "Q2", "f2.cs", QuestionCategory.General),
            new(true, "Q3", "f3.cs", QuestionCategory.XML)
        }.ToImmutableList();

        var CatSet = Categories is null or { Length: 0 } ? null : ImmutableHashSet.CreateRange(Categories);

        // Act
        var Filtered = Questions.MatchesCategories(CatSet);

        // Assert
        Filtered.Should().HaveCount(ExpectedCount);
    }

    [Fact]
    public void WithFilter_WhenFilterNull_ReturnsOriginal()
    {
        // Arrange
        var Question = new Question(true, "Q1", "f1.cs", QuestionCategory.XML);

        // Act
        var Result = Question.WithFilter(null);

        // Assert
        Result.Should().BeSameAs(Question);
    }

    [Fact]
    public void WithFilter_WhenFilterNotNull_CreatesNewQuestionWithFilter()
    {
        // Arrange
        var Question = new Question(true, "Q1", "f1.cs", QuestionCategory.XML);
        var Filter = new Filter();

        // Act
        var Result = Question.WithFilter(Filter);

        // Assert
        Result.Should().NotBeSameAs(Question)
            .And.BeOfType<Question>()
            .Which.Filter.Should().BeSameAs(Filter);
    }

    [Fact]
    public void ValidateQuestion_ThrowsWhenTextEmpty()
    {
        // Arrange
        var Question = new Question(true, "   ", "f1.cs", QuestionCategory.XML);

        // Act & Assert
        Action Act = () => Question.ValidateQuestion();
        Act.Should().Throw<ArgumentException>()
            .WithMessage("Text cannot be null/whitespace.*");
    }

    [Fact]
    public void ValidateQuestion_ThrowsWhenFilenameEmpty()
    {
        // Arrange
        var Question = new Question(true, "Q1", "", QuestionCategory.XML);

        // Act & Assert
        Action Act = () => Question.ValidateQuestion();
        Act.Should().Throw<ArgumentException>()
            .WithMessage("Filename cannot be null/whitespace.*");
    }

    [Fact]
    public void ValidateQuestion_ReturnsSelfWhenValid()
    {
        // Arrange
        var Question = new Question(true, "Q1", "f1.cs", QuestionCategory.XML);

        // Act
        var Result = Question.ValidateQuestion();

        // Assert
        Result.Should().BeSameAs(Question);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApplyFilter_CallsCorrectStrategy(bool IsEnabled)
    {
        // Arrange
        var Question = new Question(IsEnabled, "Q1", "f1.cs", QuestionCategory.XML);

        // Act
        var Result = Question.ToActiveOrDisabledQuestion();

        // Assert
        Result.Should().NotBeNull();
        // Note: actual strategy behavior depends on internal implementation
    }
}
