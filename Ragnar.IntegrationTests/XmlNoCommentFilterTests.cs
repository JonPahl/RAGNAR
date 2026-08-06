namespace Ragnar.IntegrationTests;

public class XmlNoCommentFilterTests
{
    [Fact]
    public void Filter_ShouldReturnFilterWithEmptyCommentAndMissingCommentConditions()
    {
        // Act
        var Filter = XmlEmptyCommentFilter.Filter(QuestionCategory.XML);

        // Assert
        Filter.Should().NotBeNull();
        Filter.Should.Should().HaveCount(2);

        var FirstCond = Filter.Should[0];
        FirstCond.Field.Should().NotBeNull();
        FirstCond.Field!.Key.Should().Be("Comment");
        FirstCond.Field.Match!.Text.Should().BeEmpty();

        var SecondCond = Filter.Should[1];
        SecondCond.IsEmpty.Should().NotBeNull();
        SecondCond.IsEmpty!.Key.Should().Be("Comment");
    }

    [Fact]
    public void Filter_ShouldReturnFilterWithEmptyAndMissingCommentConditions()
    {
        // Act
        var Filter = XmlEmptyCommentFilter.Filter(QuestionCategory.XML);

        // Assert
        Assert.NotNull(Filter);
        Assert.Equal(2, Filter.Should.Count);

        // First condition: Match empty comment
        var MatchCondition = Assert.IsType<Condition>(Filter.Should[0]);
        var FieldCondition = Assert.IsType<FieldCondition>(MatchCondition.Field);
        Assert.Equal("Comment", FieldCondition.Key);
        Assert.Equal(string.Empty, FieldCondition.Match!.Text);

        // Second condition: IsEmpty for comment
        var IsEmptyCondition = Assert.IsType<Condition>(Filter.Should[1]);
        Assert.NotNull(IsEmptyCondition.IsEmpty);
        Assert.Equal("Comment", IsEmptyCondition.IsEmpty!.Key);
    }
}
