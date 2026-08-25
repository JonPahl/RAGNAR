namespace Ragnar.Tests;

public class XmlCommentFilterStrategyTests
{
    [Fact]
    public void SupportedCategory_ReturnsCorrectEnum()
    {
        var strategy = new XmlCommentFilterStrategy();
        strategy.SupportedCategory.Should().Be(QuestionCategory.XML_SINGLE);
    }

    [Fact]
    public void CreateFilter_ReturnsValidFilter()
    {
        var strategy = new XmlCommentFilterStrategy();
        var filter = strategy.CreateFilter(QuestionCategory.XML_SINGLE, 100);
        filter.Should().NotBeNull();
    }

    [Fact]
    public void SupportedCategory_ReturnsXML_SINGLE()
    {
        var strategy = new XmlCommentFilterStrategy();
        Assert.Equal(QuestionCategory.XML_SINGLE, strategy.SupportedCategory);
    }

    [Fact]
    public void CreateFilter_ReturnsValidFilterObject()
    {
        var strategy = new XmlCommentFilterStrategy();
        var filter = strategy.CreateFilter(QuestionCategory.XML_SINGLE, 0);

        filter.Should().NotBeNull();
    }
}
