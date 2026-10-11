namespace Ragnar.Tests;

public class FilterBuilderTests
{
    private readonly FilterBuilder _sut;

    public FilterBuilderTests()
    {
        _sut = new FilterBuilder();
    }

    [Fact]
    public void IsEmptyWithNoConditionsShouldReturnTrue()
    {
        Assert.True(_sut.IsEmpty);
    }

    [Fact]
    public void IsEmptyAfterAddMatchShouldReturnFalse()
    {
        _sut.AddMatch("Field", "Value");

        Assert.False(_sut.IsEmpty);
    }

    [Fact]
    public void IsEmptyAfterAddExclusionShouldReturnFalse()
    {
        _sut.AddExclusion("Field", "Value");

        Assert.False(_sut.IsEmpty);
    }

    [Fact]
    public void AddMatchShouldAddToMustConditions()
    {
        // Act
        var result = _sut.AddMatch("Category", "XML");

        // Assert
        Assert.Same(_sut, result);
        Assert.False(_sut.IsEmpty);
    }

    [Fact]
    public void AddExclusionShouldAddToMustNotConditions()
    {
        // Act
        var result = _sut.AddExclusion("Category", "Skip");

        // Assert
        Assert.Same(_sut, result);
        Assert.False(_sut.IsEmpty);
    }

    [Fact]
    public void AddRangeWithGteShouldCreateFilter()
    {
        // Act
        var result = _sut.AddRange("Length", gte: 10);

        // Assert
        Assert.Same(_sut, result);
        Assert.False(_sut.IsEmpty);
    }

    [Fact]
    public void AddRangeWithLteShouldCreateFilter()
    {
        // Act
        var result = _sut.AddRange("Length", lte: 100);

        // Assert
        Assert.Same(_sut, result);
        Assert.False(_sut.IsEmpty);
    }

    [Fact]
    public void AddRangeWithBothBoundsShouldCreateFilter()
    {
        // Act
        var result = _sut.AddRange("Length", gte: 10, lte: 100);

        // Assert
        Assert.Same(_sut, result);
        Assert.False(_sut.IsEmpty);
    }

    [Fact]
    public void AddRangeWithNoBoundsShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => _sut.AddRange("Length"));
    }

    [Fact]
    public void AddExceptKeywordsShouldAddCondition()
    {
        // Act
        var result = _sut.AddExceptKeywords("Category", "Test", "Tests");

        // Assert
        Assert.Same(_sut, result);
        Assert.False(_sut.IsEmpty);
    }

    [Fact]
    public void AddExclusionRangeShouldAddToMustNot()
    {
        // Act
        var result = _sut.AddExclusionRange("Length", gte: 50, lte: 200);

        // Assert
        Assert.Same(_sut, result);
        Assert.False(_sut.IsEmpty);
    }

    [Fact]
    public void BuildWithNoConditionsShouldThrow()
    {
        Assert.Throws<InvalidOperationException>(() => _sut.Build());
    }

    [Fact]
    public void BuildWithConditionsShouldReturnFilter()
    {
        // Arrange
        _sut.AddMatch("Field", "Value");

        // Act
        var result = _sut.Build();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Must);
    }

    [Fact]
    public void BuildWithExclusionsShouldIncludeMustNot()
    {
        // Arrange
        _sut.AddExclusion("Field", "BadValue");

        // Act
        var result = _sut.Build();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.MustNot);
    }

    [Fact]
    public void BuildWithMultipleConditionsShouldIncludeAll()
    {
        // Arrange
        _sut.AddMatch("A", "1")
           .AddMatch("B", "2")
           .AddExclusion("C", "3");

        // Act
        var result = _sut.Build();

        // Assert
        Assert.Equal(2, result.Must.Count);
        Assert.Equal(1, result.MustNot.Count);
    }
}
