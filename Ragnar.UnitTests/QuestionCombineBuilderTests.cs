//using Ragnar.Builder;

//namespace Ragnar.Tests;

//public class QuestionCombineBuilderTests
//{
//    private static (QuestionCombineBuilder builder, Mock<IOptions<RagnarConfig>> opts) CreateBuilder(
//        IEnumerable<Ragnar.Core.Model.Question>? preloaded = null,
//        List<string>? categories = null)
//    {
//        var config = new RagnarConfig
//        {
//            ApplicationOptions = new ApplicationOptions
//            {
//                SourceDirectory = "",
//                VectorStoreName = "",
//                CategoriesToProcess = categories ?? new List<string>()
//            }
//        };
//        var mockOpts = new Mock<IOptions<RagnarConfig>>();
//        mockOpts.Setup(o => o.Value).Returns(config);

//        var builder = new QuestionCombineBuilder(mockOpts.Object);
//        if (preloaded != null)
//            foreach (var q in preloaded)
//                builder.Questions.Add(q);

//        return (builder, mockOpts);
//    }

//    [Fact]
//    public void BuildReturnsEmptyListWhenNoQuestions()
//    {
//        var (builder, _) = CreateBuilder();
//        var result = builder.Build();
//        Assert.Empty(result);
//    }

//    [Fact]
//    public void BuildFiltersOutDisabledQuestions()
//    {
//        var (builder, _) = CreateBuilder();
//        builder.Questions.Add(new Ragnar.Core.Model.Question { IsEnabled = true, Category = QuestionCategory.General, Filename = "a.txt" });
//        builder.Questions.Add(new Ragnar.Core.Model.Question { IsEnabled = false, Category = QuestionCategory.General, Filename = "b.txt" });
//        builder.Questions.Add(new Ragnar.Core.Model.Question { IsEnabled = true, Category = QuestionCategory.General, Filename = "c.txt" });

//        var result = builder.Build();

//        Assert.Equal(2, result.Count);
//        Assert.DoesNotContain(result, q => q.Filename == "b.txt");
//    }

//    [Fact]
//    public void BuildSortsByCategoryThenFilename()
//    {
//        var (builder, _) = CreateBuilder();
//        builder.Questions.Add(new Question { IsEnabled = true, Category = QuestionCategory.XML, Filename = "z.txt" });
//        builder.Questions.Add(new Question { IsEnabled = true, Category = QuestionCategory.General, Filename = "b.txt" });
//        builder.Questions.Add(new Question { IsEnabled = true, Category = QuestionCategory.General, Filename = "a.txt" });

//        var result = builder.Build();

//        Assert.Equal("a.txt", result[[0]].Filename);
//        Assert.Equal("b.txt", result[[1]].Filename);
//        Assert.Equal("z.txt", result[[2]].Filename);
//    }

//    [Fact]
//    public void BuildReturnsReadOnlyList()
//    {
//        var (builder, _) = CreateBuilder();
//        builder.Questions.Add(new Question { IsEnabled = true, Category = QuestionCategory.General, Filename = "a.txt" });

//        var result = builder.Build();

//        // IReadOnlyList<T> derived from List<T>.AsReadOnly() should throw on Add
//        var readOnly = Assert.IsAssignableFrom<System.Collections.ObjectModel.ReadOnlyCollection<Question>>(
//            (System.Collections.IList)result) ??
//        Assert.IsAssignableFrom<IReadOnlyList<Question>>(result);

//        // Verify it is indeed read-only by checking type
//        Assert.IsAssignableFrom<IReadOnlyList<Question>>(result);
//    }

//    [Fact]
//    public void WithCategoryFilterKeepsAllWhenCategoriesListIsEmpty()
//    {
//        var (builder, _) = CreateBuilder(categories: new List<string>());
//        builder.Questions.Add(new Question { IsEnabled = true, Category = QuestionCategory.General, Filename = "a.txt" });
//        builder.Questions.Add(new Question { IsEnabled = true, Category = QuestionCategory.XML, Filename = "b.txt" });

//        var result = builder.WithCategoryFilter();

//        Assert.Same(builder, result);
//        Assert.Equal(2, builder.Questions.Count);
//    }

//    [Fact]
//    public void WithCategoryFilterKeepsAllWhenCategoriesListIsNull()
//    {
//        var (builder, _) = CreateBuilder(categories: null);
//        builder.Questions.Add(new Question { IsEnabled = true, Category = QuestionCategory.General, Filename = "a.txt" });

//        var result = builder.WithCategoryFilter();

//        Assert.Same(builder, result);
//        Assert.Equal(1, builder.Questions.Count);
//    }

//    [Fact]
//    public void WithCategoryFilterFiltersToSpecifiedCategory()
//    {
//        var (builder, _) = CreateBuilder(categories: new List<string> { nameof(QuestionCategory.XML) });
//        builder.Questions.Add(new Core.Model.Question { IsEnabled = true, Category = QuestionCategory.General, Filename = "a.txt" });
//        builder.Questions.Add(new Ragnar.Core.Model.Question { IsEnabled = true, Category = QuestionCategory.XML, Filename = "b.txt" });

//        var result = builder.WithCategoryFilter();

//        Assert.Same(builder, result);
//        Assert.Single(builder.Questions);
//        Assert.Equal(QuestionCategory.XML, builder.Questions[[0]].Category);
//    }

//    [Fact]
//    public void WithCategoryFilterFiltersToMultipleCategories()
//    {
//        var (builder, _) = CreateBuilder(categories: new List<string> { nameof(QuestionCategory.XML), nameof(QuestionCategory.General) });
//        builder.Questions.Add(new Question { IsEnabled = true, Category = QuestionCategory.General, Filename = "a.txt" });
//        builder.Questions.Add(new Question { IsEnabled = true, Category = QuestionCategory.XML, Filename = "b.txt" });
//        builder.Questions.Add(new Question { IsEnabled = true, Category = QuestionCategory.Unit, Filename = "c.txt" });

//        builder.WithCategoryFilter();

//        Assert.Equal(2, builder.Questions.Count);
//        Assert.DoesNotContain(builder.Questions, q => q.Category == QuestionCategory.Unit);
//    }
//}
