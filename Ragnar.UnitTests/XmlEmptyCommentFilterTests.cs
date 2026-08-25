namespace Ragnar.Tests;

public class XmlEmptyCommentFilterTests
{
    [Fact]
    public void Filter_ReturnsValidFilter_ForXML()
    {
        var filter = XmlEmptyCommentFilter.BuildFilter(QuestionCategory.XML);
        filter.Should().NotBeNull();
    }

    [Theory]
    [InlineData((QuestionCategory)999)] // Invalid enum value
    public void Filter_Throws_ArgumentException_ForInvalidCategory(QuestionCategory Category)
    {
        Assert.Throws<ArgumentException>(() => XmlEmptyCommentFilter.BuildFilter(Category));
    }
}
