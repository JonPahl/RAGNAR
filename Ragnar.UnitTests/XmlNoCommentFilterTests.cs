namespace RAGNAR.UnitTests;

public class XmlNoCommentFilterTests
{

    [Fact]
    public void Filter_ShouldReturnFilterWithEmptyCommentConditions()
    {
        // Act
        var Filter = XmlEmptyCommentFilter.Filter(QuestionCategory.XML);

        // Assert
        Assert.NotNull(Filter);
        Assert.Equal(2, Filter.Should.Count);

        var FirstCondition = Filter.Should[0];
        Assert.NotNull(FirstCondition.Field);
        Assert.Equal("Comment", FirstCondition.Field.Key);
        Assert.NotNull(FirstCondition.Field.Match);
        Assert.Equal(string.Empty, FirstCondition.Field.Match.Text);

        var SecondCondition = Filter.Should[1];
        Assert.NotNull(SecondCondition.IsEmpty);
        Assert.Equal("Comment", SecondCondition.IsEmpty.Key);
    }

    [Fact]
    public void Filter_ShouldReturnFilterWithTwoConditions_ForAnyCategory()
    {
        // Act
        var Result = XmlEmptyCommentFilter.Filter(QuestionCategory.XML);

        // Assert
        Result.Should().NotBeNull();
        Result.Should.Should().HaveCount(2);

        // First condition: Comment == ""
        Result.Should[0].Should().BeOfType<Condition>();
        Result.Should[0].Field.Should().NotBeNull();
        Result.Should[0].Field.Key.Should().Be("Comment");
        Result.Should[0].Field.Match.Text.Should().Be(string.Empty);

        // Second condition: Comment is empty
        Result.Should[1].Should().BeOfType<Condition>();
        Result.Should[1].IsEmpty.Key.Should().Be("Comment");
    }

    [Theory]
    [InlineData(QuestionCategory.XML)]
    public void Filter_ShouldReturnCorrectFilter_ForXmlCategory(QuestionCategory Category)
    {
        // Act
        var Filter = XmlEmptyCommentFilter.Filter(Category);

        // Assert
        Filter.Should().NotBeNull();
        Filter.Should.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(QuestionCategory.Other)]
    [InlineData(QuestionCategory.General)]
    public void Filter_ShouldNotThrow_ForNonXmlCategory(QuestionCategory Category)
    {
        // Act & Assert
        Action Act = () => XmlEmptyCommentFilter.Filter(Category);
        Act.Should().NotThrow(); // Note: current implementation doesn't validate category
    }
}
