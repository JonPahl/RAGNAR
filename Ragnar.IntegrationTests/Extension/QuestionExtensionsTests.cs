namespace Ragnar.IntegrationTests.Extension;

public class QuestionExtensionsTests
{
    [Fact]
    public void ActiveOnlyReturnsOnlyEnabledQuestions()
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1", QuestionCategory.XML),
            new(false, "Q2", "f2", QuestionCategory.XML),
            new(true, "Q3", "f3", QuestionCategory.General)
        };

        // Act
        var Active = Questions.ActiveOnly();

        // Assert
        Assert.Equal(2, Active.Count);
        Assert.All(Active, Q => Assert.True(Q.IsEnabled));
    }

    [Fact]
    public void InActiveOnlyReturnsOnlyDisabledQuestions()
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1", QuestionCategory.General),
            new(false, "Q2", "f2", QuestionCategory.XML),
            new(true, "Q3", "f3", QuestionCategory.XML)
        };

        // Act        
        var Inactive = Questions.InActiveOnly();

        // Assert
        Assert.Single(Inactive);
        Assert.False(Inactive[0].IsEnabled);
    }

    [Fact]
    public void WithCategoryWithEmptyHashSetReturnsAll()
    {
        // Arrange
        var Items = new List<Question> { new(true, "Q1", "f1", QuestionCategory.XML) };
        var Categories = ImmutableHashSet<QuestionCategory>.Empty;
        var Questions = Items.AsReadOnly();

        // Act
        var Result = Questions.MatchesCategories(Categories);

        // Assert
        Assert.Single(Result);
    }

    [Fact]
    public void WithCategoryFiltersByCategory()
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1", QuestionCategory.XML),
            new(true, "Q2", "f2", QuestionCategory.XML),
            new(true, "Q3", "f3", QuestionCategory.General)
        };
        var Categories = ImmutableHashSet.Create(QuestionCategory.XML);

        // Act
        var Result = Questions.MatchesCategories(Categories);

        // Assert
        Assert.Equal(2, Result.Count);
        Assert.All(Result, Q => Assert.Equal(QuestionCategory.XML, Q.Category));
    }

    [Fact]
    public void SetFilterWithNullFilterReturnsOriginal()
    {
        // Arrange
        var Question = new Question(true, "Q1", "f1", QuestionCategory.XML);

        // Act
        var Result = Question.WithFilter(null);

        // Assert
        Assert.Same(Question, Result);
    }

    [Fact]
    public void SetFilterWithNonNullFilterCreatesNewQuestion()
    {
        // Arrange
        var Question = new Question(true, "Q1", "f1", QuestionCategory.XML);
        var Filter = new Filter();

        // Act
        var Result = Question.WithFilter(Filter);

        // Assert
        Assert.NotSame(Question, Result);
        Assert.Equal(Question.IsEnabled, Result.IsEnabled);
        Assert.Equal(Question.Text, Result.Text);
        Assert.Equal(Question.Filename, Result.Filename);
        Assert.Equal(Question.Category, Result.Category);
        Assert.NotNull(Result.Filter);
    }

    [Fact]
    public void ValidateQuestionWithValidQuestionReturnsSame()
    {
        // Arrange
        var Question = new Question(true, "Q1", "f1", QuestionCategory.XML);

        // Act & Assert
        Assert.Same(Question, Question.ValidateQuestion());
    }

    [Fact]
    public void ActiveOnly_ReturnsOnlyEnabledQuestions()
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
        Assert.Equal(2, Active.Count);
        Assert.Contains(Active, Q => Q.Text == "Q1");
        Assert.Contains(Active, Q => Q.Text == "Q3");
    }

    [Fact]
    public void InActiveOnly_ReturnsOnlyDisabledQuestions()
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML),
            new(false, "Q2", "f2.cs", QuestionCategory.XML)
        };

        // Act
        var Inactive = Questions.InActiveOnly();

        // Assert
        Assert.Single(Inactive);
        Assert.Equal("Q2", Inactive[0].Text);
    }

    [Fact]
    public void WithCategory_ThrowsOnNull()
    {
        // Arrange
        var Questions = new List<Question>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Questions.MatchesCategories(null));
    }

    [Theory]
    [InlineData(null, 2)]
    [InlineData(new QuestionCategory[] { }, 2)]
    [InlineData(new[] { QuestionCategory.XML }, 1)]
    public void WithCategory_FiltersByCategory(QuestionCategory[]? Categories, int ExpectedCount)
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML),
            new(false, "Q2", "f2.cs", QuestionCategory.Other)
        };

        var CatSet = Categories is null or { Length: 0 } ? null : ImmutableHashSet.Create(Categories);

        // Act
        var Filtered = Questions.MatchesCategories(CatSet ?? []);

        // Assert
        Assert.Equal(ExpectedCount, Filtered.Count);
    }

    [Fact]
    public void SetFilterWithNullFilterReturnsSameQuestion()
    {
        // Arrange
        var Question = new Question(true, "Q", "f.cs", QuestionCategory.XML);

        // Act
        var Result = Question.WithFilter(null);

        // Assert
        Assert.Same(Question, Result);
    }

    [Fact]
    public void SetFilterWithFilterCreatesNewQuestion()
    {
        // Arrange
        var Question = new Question(true, "Q", "f.cs", QuestionCategory.XML);
        var Filter = new XmlCommentFilterStrategy().CreateFilter(QuestionCategory.XML, 0);

        // Act
        var Result = Question.WithFilter(Filter);

        // Assert
        Assert.NotSame(Question, Result);
        Assert.Equal(Question.IsEnabled, Result.IsEnabled);
        Assert.Equal(Question.Text, Result.Text);
        Assert.Equal(Question.Filename, Result.Filename);
        Assert.Equal(Question.Category, Result.Category);
        Assert.NotNull(Result.Filter);
    }

    [Fact]
    public void ValidateQuestionThrowsOnNullProperties()
    {
        // Arrange
        var Question = new Question(true, null!, "f.cs", QuestionCategory.XML);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Question.ValidateQuestion());
    }

    [Fact]
    public void ValidateQuestion_ThrowsOnWhitespaceProperties()
    {
        // Arrange
        var Question = new Question(true, "  ", "f.cs", QuestionCategory.XML);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Question.ValidateQuestion());
    }

    [Fact]
    public void ValidateQuestion_ReturnsSameInstance_WhenValid()
    {
        // Arrange
        var Question = new Question(true, "Q", "f.cs", QuestionCategory.XML);

        // Act
        var Result = Question.ValidateQuestion();

        // Assert
        Assert.Same(Question, Result);
    }


    [Fact]
    public void WithCategory_WithNullCategories_ThrowsArgumentNullException()
    {
        // Arrange
        var Questions = new List<Question> { new(true, "", "") }.AsReadOnly();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Questions.MatchesCategories(null));
    }

    [Fact]
    public void WithCategory_WithEmptyCategories_ReturnsAllQuestions()
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true,"","",QuestionCategory.XML),
            new(true,"","",QuestionCategory.General)
        }.AsReadOnly();

        var Categories = ImmutableHashSet<QuestionCategory>.Empty;

        // Act
        var Result = Questions.MatchesCategories(Categories);

        // Assert
        Result.Should().HaveCount(2);
    }

    [Fact]
    public void WithCategory_WithMatchingCategories_ReturnsFiltered()
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "","", QuestionCategory.XML),
            new(true, "","", QuestionCategory.Other)
        }.AsReadOnly();

        var Categories = ImmutableHashSet.Create(QuestionCategory.XML);

        // Act
        var Result = Questions.MatchesCategories(Categories);

        // Assert
        Result.Should().HaveCount(1).And.OnlyContain(Q => Q.Category == QuestionCategory.XML);
    }

    [Fact]
    public void ValidateQuestion_WithValidQuestion_ReturnsSame()
    {
        // Arrange
        var Question = new Question(
            IsEnabled: true,
            Text: "Is this valid?",
            Filename: "Program.cs",
            Category: QuestionCategory.XML);

        // Act
        var Result = Question.ValidateQuestion();

        // Assert
        Result.Should().BeSameAs(Question);
    }

    [Fact]
    public void ValidateQuestionWithNullTextThrowsArgumentNullException()
    {
        // Arrange
        var Question = new Question(
            IsEnabled: true,
            Text: null!,
            Filename: "Program.cs",
            Category: QuestionCategory.XML);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Question.ValidateQuestion());
    }

    [Fact]
    public void ValidateQuestion_WithWhitespaceText_ThrowsArgumentException()
    {
        // Arrange
        var Question = new Question(
            IsEnabled: true,
            Text: "   ",
            Filename: "Program.cs",
            Category: QuestionCategory.XML);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Question.ValidateQuestion());
    }

    [Fact]
    public void SetFilter_WithNullFilter_ReturnsOriginal()
    {
        // Arrange
        var Original = new Question(
            IsEnabled: true,
            Text: "Test",
            Filename: "Program.cs",
            Category: QuestionCategory.XML);

        // Act
        var Result = Original.WithFilter(null);

        // Assert
        Result.Should().BeSameAs(Original);
    }

    [Fact]
    public void SetFilter_WithNonNullFilter_CreatesNewQuestionWithFilter()
    {
        // Arrange
        var Original = new Question(
            IsEnabled: true,
            Text: "Test",
            Filename: "Program.cs",
            Category: QuestionCategory.XML);

        var Filter = new XmlCommentLengthFilterStrategy().CreateFilter(QuestionCategory.XML, 100);

        // Act
        var Result = Original.WithFilter(Filter);

        // Assert
        Result.Should().NotBeSameAs(Original);
        Result.Filter.Should().BeSameAs(Filter);
    }

    [Theory]
    [InlineData(true, typeof(Question))]
    [InlineData(false, typeof(Question))]
    public void ProcessQuestion_WithEnabledFlag_ReturnsCorrectProcessedQuestion(bool IsEnabled, Type ExpectedType)
    {
        // Arrange
        var Question = new Question(
            IsEnabled: IsEnabled,
            Text: "Test",
            Filename: "Program.cs",
            Category: QuestionCategory.XML);

        // Act
        var Result = Question.ToActiveOrDisabledQuestion();

        // Assert
        Result.Should().BeOfType(ExpectedType);
    }


    [Fact]
    public void WhereCategoryIs_ThrowsWhenCategoriesNull()
    {
        // Arrange
        var Questions = new List<Question>().ToImmutableList();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Questions.MatchesCategories(null));
    }

    [Theory]
    [InlineData(null, 2)]
    [InlineData(new QuestionCategory[] { }, 2)]
    [InlineData(new[] { QuestionCategory.XML }, 1)]
    public void WhereCategoryIsFiltersByCategories(QuestionCategory[]? Categories, int ExpectedCount)
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML),
            new(true, "Q2", "f2.cs", QuestionCategory.Other)
        }.ToImmutableList();

        var CatSet = Categories is null or { Length: 0 } ? null : ImmutableHashSet.Create(Categories);

        // Act
        var Filtered = Questions.MatchesCategories(CatSet);

        // Assert
        Filtered.Should().HaveCount(ExpectedCount);
    }


    [Fact]
    public void WithFilterReturnsOriginalWhenFilterNull()
    {
        // Arrange
        var Question = new Question(true, "Q1", "f1.cs", QuestionCategory.XML);

        // Act
        var Updated = Question.WithFilter(null);

        // Assert
        Updated.Should().BeSameAs(Question);
    }

    [Fact]
    public void ValidateQuestionReturnsSelfWhenValid()
    {
        // Arrange
        var Question = new Question(true, "Q", "f.cs", QuestionCategory.XML);

        // Act
        var Validated = Question.ValidateQuestion();

        // Assert
        Validated.Should().BeSameAs(Question);
    }

    [Theory]
    [InlineData("", "file.cs", "Text cannot be null/whitespace")]
    [InlineData("  ", "file.cs", "Text cannot be null/whitespace")]
    [InlineData("What does this do?", "", "Filename cannot be null/whitespace")]
    public void ValidateQuestion_ShouldThrow_WhenInvalid(string Text, string Filename, string ExpectedMessage)
    {
        // Arrange
        var Question = new Question(true, Text, Filename, QuestionCategory.XML, null);

        // Act & Assert
        var Ex = Assert.Throws<ArgumentException>(() => Question.ValidateQuestion());
        Assert.Contains(ExpectedMessage, Ex.Message);
    }

    [Fact]
    public void ValidateQuestion_ShouldReturnSameInstance_WhenValid()
    {
        // Arrange
        var Question = new Question(true, "What does this do?", "Program.cs", QuestionCategory.XML, null);

        // Act
        var Result = Question.ValidateQuestion();

        // Assert
        Assert.Same(Question, Result);
    }

    [Fact]
    public void WithFilter_ShouldReturnNewQuestion_WhenFilterProvided()
    {
        // Arrange
        var Original = new Question(true, "What does this do?", "Program.cs", QuestionCategory.XML, null);
        var Filter = new Filter(); // dummy filter

        // Act
        var Result = Original.WithFilter(Filter);

        // Assert
        Assert.NotSame(Original, Result);
        Assert.Equal(Original.IsEnabled, Result.IsEnabled);
        Assert.Equal(Original.Text, Result.Text);
        Assert.Equal(Original.Filename, Result.Filename);
        Assert.Equal(Original.Category, Result.Category);
        Assert.NotNull(Result.Filter);
    }

    [Fact]
    public void WithFilterShouldReturnSameQuestionWhenFilterIsNull()
    {
        // Arrange
        var Original = new Question(true, "What does this do?", "Program.cs", QuestionCategory.XML, null);

        // Act
        var Result = Original.WithFilter(null);

        // Assert
        Assert.Same(Original, Result);
    }


    [Fact]
    public void MatchesCategories_ShouldThrow_WhenCategoriesIsNull()
    {
        // Arrange
        var Questions = ImmutableArray<Question>.Empty;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Questions.MatchesCategories(null));
    }

    [Fact]
    public void MatchesCategories_WithEmptyCategoriesSet_ShouldReturnAll()
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML, null),
            new(true, "Q2", "f2.cs", QuestionCategory.General, null)
        }.ToImmutableArray();

        var Categories = ImmutableHashSet<QuestionCategory>.Empty;

        // Act
        var Result = Questions.MatchesCategories(Categories);

        // Assert
        Assert.Equal(2, Result.Count);
    }

    [Fact]
    public void ToActiveOrDisabledQuestion_ShouldReturnSame_WhenEnabled()
    {
        // Arrange
        var Question = new Question(true, "Q1", "f1.cs", QuestionCategory.XML, null);

        // Act
        var Result = Question.ToActiveOrDisabledQuestion();

        // Assert
        Assert.Same(Question, Result);
    }

    [Fact]
    public void ToActiveOrDisabledQuestion_ShouldReturnSame_WhenDisabled()
    {
        // Arrange
        var Question = new Question(false, "Q1", "f1.cs", QuestionCategory.XML, null);

        // Act
        var Result = Question.ToActiveOrDisabledQuestion();

        // Assert
        Assert.Same(Question, Result);
    }

    [Fact]
    public void MatchesCategories_ShouldThrow_WhenCategoriesNull()
    {
        // Arrange
        var Questions = ImmutableArray<Question>.Empty;

        // Act & Assert
        Action Act = () => Questions.MatchesCategories(null);
        Act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void MatchesCategories_ShouldReturnAll_WhenCategoriesEmpty()
    {
        // Arrange
        var Questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML, null),
            new(true, "Q2", "f2.cs", QuestionCategory.Other, null)
        }.ToImmutableList();

        // Act
        var Filtered = Questions.MatchesCategories([]);

        // Assert
        Filtered.Should().HaveCount(2);
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

    [Fact]
    public void ToActiveOrDisabledQuestion_ShouldReturnSameInstance_WhenEnabled()
    {
        // Arrange
        var question = new Question(true, "Q", "f.cs", QuestionCategory.XML, null);

        // Act
        var result = question.ToActiveOrDisabledQuestion();

        // Assert
        result.Should().BeSameAs(question);
    }

    [Fact]
    public void ToActiveOrDisabledQuestion_ShouldReturnSameInstance_WhenDisabled()
    {
        // Arrange
        var question = new Question(false, "Q", "f.cs", QuestionCategory.XML, null);

        // Act
        var result = question.ToActiveOrDisabledQuestion();

        // Assert
        result.Should().BeSameAs(question);
    }

    [Fact]
    public void MatchesCategories_ShouldThrowWhenCategoriesNull()
    {
        // Arrange
        var questions = new List<Question>().ToImmutableList();
        ImmutableHashSet<QuestionCategory>? categories = null;

        // Act & Assert
        Action act = () => questions.MatchesCategories(categories);
        act.Should().Throw<ArgumentNullException>().WithParameterName(nameof(categories));
    }

    [Fact]
    public void MatchesCategories_ShouldReturnAllWhenCategoriesEmpty()
    {
        // Arrange
        var questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML, null),
            new(true, "Q2", "f2.cs", QuestionCategory.Other, null)
        }.ToImmutableList();
        var categories = ImmutableHashSet<QuestionCategory>.Empty;

        // Act
        var result = questions.MatchesCategories(categories);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public void MatchesCategories_ShouldFilterByCategory()
    {
        // Arrange
        var questions = new List<Question>
        {
            new(true, "Q1", "f1.cs", QuestionCategory.XML, null),
            new(true, "Q2", "f2.cs", QuestionCategory.Other, null),
            new(true, "Q3", "f3.cs", QuestionCategory.XML, null)
        }.ToImmutableList();
        var categories = ImmutableHashSet.Create(QuestionCategory.XML);

        // Act
        var result = questions.MatchesCategories(categories);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(q => q.Text == "Q1");
        result.Should().Contain(q => q.Text == "Q3");
    }








}
