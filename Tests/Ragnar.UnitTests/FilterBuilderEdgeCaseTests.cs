namespace Ragnar.Tests;

public class FilterBuilderEdgeCaseTests
{
    [Fact]
    public void BuildWithOnlyExclusionsShouldReturnFilterWithMustNot()
    {
        var builder = new FilterBuilder();
        builder.AddExclusion("Status", "deleted");

        var filter = builder.Build();

        Assert.Empty(filter.Must);
        Assert.Single(filter.MustNot);
    }

    [Fact]
    public void BuildWithMultipleExclusionsShouldIncludeAll()
    {
        var builder = new FilterBuilder();
        builder.AddExclusion("A", "1")
               .AddExclusion("B", "2")
               .AddExclusion("C", "3");

        var filter = builder.Build();

        Assert.Equal(3, filter.MustNot.Count);
    }

    [Fact]
    public void AddRangeWithOnlyGteShouldCreateCondition()
    {
        var builder = new FilterBuilder();
        var result = builder.AddRange("Score", gte: 85.0);

        Assert.Same(builder, result);
        var filter = builder.Build();
        Assert.NotEmpty(filter.Must);
    }

    [Fact]
    public void AddRangeWithOnlyLteShouldCreateCondition()
    {
        var builder = new FilterBuilder();
        var result = builder.AddRange("Score", lte: 10.0);

        Assert.Same(builder, result);
        var filter = builder.Build();
        Assert.NotEmpty(filter.Must);
    }

    [Fact]
    public void AddExceptKeywordsWithSingleKeywordShouldWork()
    {
        var builder = new FilterBuilder();
        builder.AddExceptKeywords("Tag", "skip");

        var filter = builder.Build();
        Assert.NotEmpty(filter.Must);
    }

    [Fact]
    public void ChainedOperationsShouldBuildValidFilter()
    {
        var builder = new FilterBuilder();
        builder.AddMatch("Type", "XML")
               .AddRange("Length", gte: 10, lte: 500)
               .AddExceptKeywords("Cat", "Test")
               .AddExclusion("Deprecated", "true")
               .AddExclusionRange("Score", gte: 0, lte: 5);

        var filter = builder.Build();

        Assert.Equal(3, filter.Must.Count);
        Assert.Equal(2, filter.MustNot.Count);
    }
}
