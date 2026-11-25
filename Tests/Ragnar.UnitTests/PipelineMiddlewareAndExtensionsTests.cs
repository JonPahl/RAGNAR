namespace Ragnar.Tests;

public sealed class PipelineMiddlewareAndExtensionsTests
{
    #region FluentValidationExtensions Tests

    //[Fact]
    //public async Task IsValidUriShouldReturnCorrectStatusForValidUri()
    //{
    //    // Arrange
    //    var ruleMock = new Mock<IRuleBuilder<string, string>>();
    //    ruleMock.Setup(x => x.Must(It.IsAny<Func<string, bool>>()))
    //            .Returns(Mock.Of<IRuleBuilderOptions<string, string>>());

    //    // Act
    //    var result = await ValidateRuleAsync(ruleMock, "https://example.com");

    //    // Assert
    //    ruleMock.Verify(x => x.Must(It.Is<Func<string, bool>>(f => f("https://example.com"))), Times.Once);
    //}

    //private async Task<bool> ValidateRuleAsync(Mock<IRuleBuilder<string, string>> mock, string value)
    //{
    //    bool isValid = false;
    //    // Use `out` modifier on lambda parameter as requested
    //    mock.Setup(x => x.Must(isValid)).Returns(mock.Object);
    //    return isValid;
    //}

    #endregion

    #region QdrantConditionExtensions Tests
    [Fact]
    public void MatchKeywordShouldConstructCorrectField()
    {
        // Arrange
        var dummy = new Condition(); // Extension target
        var field = "Category";
        var value = "Test";

        // Act
        var result = dummy.MatchKeyword(field, value);

        // Assert
        Assert.NotNull(result.Field);
        Assert.Equal(field, result.Field.Key);
        Assert.NotNull(result.Field.Match?.Keywords);
    }

    [Fact]
    public void AddExceptKeywordsShouldPopulateRepeatedStrings()
    {
        var dummy = new Condition();
        List<string> keywords = ["X", "Y"];

        var result = dummy.ExceptKeywords("Cat", [.. keywords]);

        Assert.NotNull(result.Field.Match?.ExceptKeywords?.Strings);
        Assert.Equal(keywords.Count, result.Field.Match.ExceptKeywords.Strings.Count);
    }
    #endregion

    #region StylesExtensions Tests
    [Fact]
    public void EnsureValidStyleShouldReturnPlainOnNull()
    {
        // Arrange
        Style? style = null;

        // Act & Assert (Uses `in` modifier on lambda parameter)
        var result = ApplyStyle(style, s => DomainExtensions.EnsureValidStyle(s));
        Assert.Equal(Style.Plain, result);
    }

    [Fact]
    public void EnsureValidStyleShouldReturnInputOnNonNull()
    {
        // Arrange
        Style input = new(Color.Red);

        // Act
        var result = ApplyStyle(input, static s => DomainExtensions.EnsureValidStyle(s));

        // Assert
        Assert.Equal(input, result);
    }

    private T ApplyStyle<T>(in T value, Func<T?, T> transform) => transform(value)!;
    #endregion
}
