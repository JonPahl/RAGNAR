namespace Ragnar.IntegrationTests;

/// <summary>
/// Integration tests verifying filter builder composition and strategy integration.
/// </summary>
public class FilterBuilderIntegrationTests
{
    #region Fluent Builder Composition

    [Fact]
    public void ChainedFiltersShouldProduceCorrectQdrantFilter()
    {
        // Arrange & Act
        var filter = new QdrantFilterBuilder()
            .AddMustMatch("Category", "XML")
            .AddRange("CommentLength", gte: 50)
            .AddExceptKeywords("Category", "Test", "Tests")
            .Build();

        // Assert
        Assert.NotNull(filter.Must);
        Assert.NotNull(filter.MustNot);
        Assert.True(filter.Must.Count >= 2); // Category match + range
        Assert.True(filter.MustNot.Count >= 1); // except keywords
    }

    [Fact]
    public void BuildWithMixedConditionsShouldPreserveAll()
    {
        // Act
        var filter = new QdrantFilterBuilder()
            .AddMustMatch("Type", "Class")
            .AddMustNotMatch("Type", "Interface")
            .AddRange("Size", gte: 10, lte: 500)
            .AddMustNotRange("Size", gte: 1000, lte: 2000)
            .Build();

        // Assert
        Assert.Equal(2, filter.Must.Count); // Type match + range
        Assert.Equal(2, filter.MustNot.Count); // Type exclusion + range exclusion
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void BuildWithNoConditionsShouldThrow()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => new QdrantFilterBuilder().Build());
    }

    [Fact]
    public void AddRangeWithNoBoundsShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new QdrantFilterBuilder().AddRange("Field"));
    }

    [Fact]
    public void IsEmptyShouldReflectState()
    {
        // Arrange
        var builder = new QdrantFilterBuilder();

        // Assert initial
        Assert.True(builder.IsEmpty);

        // Act
        builder.AddMustMatch("A", "B");

        // Assert
        Assert.False(builder.IsEmpty);
    }

    #endregion

    #region Strategy + Builder Integration

    [Fact]
    public void XmlStrategyFilterShouldBeValidQdrantFilter()
    {
        // Arrange
        var strategy = new XmlCommentFilterStrategy(Mock.Of<ILogger>());

        // Act
        var filter = strategy.CreateFilter(25);

        // Assert – filter is usable (not null, has conditions)
        Assert.NotNull(filter);
        Assert.True(filter.Must is { Count: > 0 } || filter.MustNot is { Count: > 0 });
    }

    [Fact]
    public void FilterSelectorShouldConsistentlyReturnSameStrategy()
    {
        // Arrange
        var logger = Mock.Of<ILogger>();
        var selector = new QuestionFilterSelector([]);

        // Act
        var f1 = selector.FindFilter(QuestionCategory.XML, 10);
        var f2 = selector.FindFilter(QuestionCategory.XML, 10);

        // Assert
        Assert.NotNull(f1);
        Assert.NotNull(f2);
        Assert.Equal(f1.Must?.Count, f2.Must?.Count);
    }

    #endregion
}
