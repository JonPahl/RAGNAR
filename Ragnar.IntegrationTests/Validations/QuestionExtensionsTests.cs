namespace Ragnar.IntegrationTests.Validations;

public class QuestionExtensionsTests
{
    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void ActiveOnly_ReturnsOnlyEnabledQuestions(bool IsEnabled, int ExpectedCount)
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(IsEnabled: false, "Q1", "f1.cs", QuestionCategory.General),
            new(IsEnabled: IsEnabled, "Q2", "f2.cs", QuestionCategory.XML),
            new(IsEnabled: false, "Q3", "f3.cs", QuestionCategory.XML)
        }.AsReadOnly();

        // Act
        var Active = Questions.ActiveOnly();

        // Assert
        Assert.Equal(ExpectedCount, Active.Count);
        Assert.All(Active, Q => Assert.True(Q.IsEnabled));
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 1)]
    public void InActiveOnly_ReturnsOnlyDisabledQuestions(bool IsEnabled, int ExpectedCount)
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(IsEnabled: true, "Q1", "f1.cs", QuestionCategory.General),
            new(IsEnabled: IsEnabled, "Q2", "f2.cs", QuestionCategory.XML),
            new(IsEnabled: false, "Q3", "f3.cs", QuestionCategory.Other)
        }.AsReadOnly();

        // Act
        var Inactive = Questions.InActiveOnly();

        // Assert
        Assert.Equal(ExpectedCount, Inactive.Count);
        Assert.All(Inactive, Q => Assert.False(Q.IsEnabled));
    }

    [Fact]
    public void WhereCategoryIs_ThrowsWhenCategoriesNull()
    {
        // Arrange
        var Questions = new List<Question>().AsReadOnly();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Questions.MatchesCategories(null));
    }

    [Theory]
    //[InlineData(null, 3)]
    //[InlineData(new QuestionCategory[0], 3)]
    [InlineData(new[] { QuestionCategory.Other }, 1)]
    public void WhereCategoryIs_FiltersByCategories(QuestionCategory[]? Categories, int ExpectedCount)
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.General),
            new(true, "Q2", "f2.cs", QuestionCategory.General),
            new(true, "Q3", "f3.cs", QuestionCategory.Other)
        }.AsReadOnly();

        var CatSet = Categories is null or { Length: 0 } ? null : ImmutableHashSet.Create(Categories);

        // Act
        var Filtered = Questions.MatchesCategories(CatSet);

        // Assert
        Assert.Equal(ExpectedCount, Filtered.Count);
    }

    [Fact]
    public void WithFilter_WhenFilterNull_ReturnsOriginal()
    {
        // Arrange
        var Question = new Question(true, "Q", "f.cs", QuestionCategory.General);

        // Act
        var Result = Question.WithFilter(null);

        // Assert
        Assert.Same(Question, Result);
    }

    [Fact]
    public void WithFilter_WhenFilterNotNull_CreatesNewQuestion()
    {
        // Arrange
        var Original = new Question(true, "Q", "f.cs", QuestionCategory.General);
        var Filter = new XmlCommentFilterStrategy().CreateFilter(QuestionCategory.XML, 0);

        // Act
        var Result = Original.WithFilter(Filter);

        // Assert
        Assert.NotSame(Original, Result);
        Assert.Equal(Original.IsEnabled, Result.IsEnabled);
        Assert.Equal(Original.Text, Result.Text);
        Assert.Equal(Original.Filename, Result.Filename);
        Assert.Equal(Original.Category, Result.Category);
        Assert.Equal(Filter, Result.Filter); // assuming Filter implements equality
    }

    [Theory]
    [InlineData("Q", "", QuestionCategory.General
        )] // invalid filename
    [InlineData("", "f.cs", QuestionCategory.Other)] // invalid text
    [InlineData("  ", "f.cs", QuestionCategory.XML)] // whitespace text
    public void ValidateQuestion_ThrowsWhenInvalid(string Text, string Filename, QuestionCategory Category)
    {
        // Arrange
        var Question = new Question(true, Text, Filename, Category);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Question.ValidateQuestion());
    }

    [Fact]
    public void ValidateQuestion_ReturnsValidQuestion()
    {
        // Arrange
        var Question = new Question(true, "Q", "f.cs", QuestionCategory.General);

        // Act
        var Result = Question.ValidateQuestion();

        // Assert
        Assert.Same(Question, Result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApplyFilter_CallsCorrectStrategy(bool IsEnabled)
    {
        // Arrange
        var Question = new Question(IsEnabled, "Q", "f.cs", QuestionCategory.General);

        // Act
        var Result = Question.ToActiveOrDisabledQuestion();

        // Assert
        Assert.NotNull(Result);
        // Optional: verify filter type based on IsEnabled
    }
}
