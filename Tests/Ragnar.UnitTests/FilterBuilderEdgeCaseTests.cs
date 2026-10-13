namespace Ragnar.Tests;

public class FilterBuilderEdgeCaseTests
{
    private readonly QdrantFilterBuilder _sut;

    public FilterBuilderEdgeCaseTests()
    {
        _sut = new QdrantFilterBuilder();
    }

    [Fact]
    public void AddRangeShouldThrowWhenBothGteAndLteAreNull()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _sut.AddRange("Field"));
        Assert.Contains("at least one", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddExclusionRangeShouldThrowWhenBothBoundsAreNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.AddExclusionRange("Field"));
    }

    [Fact]
    public void AddRangeWithOnlyGteShouldSucceed()
    {
        // Act
        var result = _sut.AddRange("Len", gte: 10.0);

        // Assert
        Assert.Same(_sut, result);
        Assert.False(_sut.IsEmpty);
    }

    [Fact]
    public void AddRangeWithOnlyLteShouldSucceed()
    {
        // Act
        var result = _sut.AddRange("Len", lte: 100.0);

        // Assert
        Assert.Same(_sut, result);
        Assert.False(_sut.IsEmpty);
    }

    [Fact]
    public void BuildWithOnlyMustNotConditionsShouldReturnFilterWithEmptyMust()
    {
        // Arrange
        _sut.AddExclusion("Field", "Value");

        // Act
        var result = _sut.Build();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.MustNot);
    }

    [Fact]
    public void ChainingMultipleConditionsShouldWork()
    {
        // Act
        var result = _sut
            .AddMatch("A", "1")
            .AddExclusion("B", "2")
            .AddRange("C", gte: 1.0, lte: 100.0)
            .AddExceptKeywords("D", "X", "Y")
            .AddExclusionRange("E", gte: 5.0);

        // Assert
        Assert.Same(_sut, result);
        Assert.False(_sut.IsEmpty);

        var filter = _sut.Build();
        Assert.Equal(3, filter.Must.Count);   // A, C, D
        Assert.Equal(2, filter.MustNot.Count); // B, E
    }
}
