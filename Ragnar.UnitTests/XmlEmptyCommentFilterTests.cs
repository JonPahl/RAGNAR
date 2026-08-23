namespace Ragnar.Tests;

public class XmlEmptyCommentFilterTests
{
    [Theory]
    [InlineData((QuestionCategory)99)]
    public void Filter_Throws_InvalidEnumArgumentException_For_Invalid_Category(QuestionCategory Category)
    {
        // Act & Assert
        Assert.Throws<InvalidEnumArgumentException>(() => XmlEmptyCommentFilter.Filter(Category));
    }

    [Fact]
    public void Filter_Creates_Filter_With_Correct_Conditions_For_XML()
    {
        // Act
        var filter = XmlEmptyCommentFilter.Filter(QuestionCategory.XML);

        // Assert
        filter.Should().NotBeNull();
        filter.Should.Should().HaveCount(2);

        var fieldCondition = filter.Should.OfType<Condition>().First(c => c.Field != null);
        fieldCondition.Field.Key.Should().Be("Comment");
        fieldCondition.Field.Match.Text.Should().BeEmpty();

        var isEmptyCondition = filter.Should.OfType<Condition>().First(c => c.IsEmpty != null);
        isEmptyCondition.IsEmpty.Key.Should().Be("Comment");
    }
}
