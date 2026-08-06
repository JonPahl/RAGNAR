namespace Ragnar.IntegrationTests.Extension;

public class CodeDocumentExtensionsTests_Integration
{
    [Fact]
    public void ToPayloadDictionary_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var Doc = new CodeDocument(
            FileName: "Test.cs",
            ElementType: "class",
            ElementName: "TestClass",
            Comment: "summary",
            CommentLength: 7,
            Code: "public class TestClass {}",
            Category: nameof(QuestionCategory.XML)
        );

        // Act
        var Payload = Doc.ToPayloadDictionary;

        // Assert
        Payload.Should().HaveCount(7);
        Payload[nameof(Doc.FileName)].StringValue.Should().Be("Test.cs");
        Payload[nameof(Doc.ElementType)].StringValue.Should().Be("class");
        Payload[nameof(Doc.ElementName)].StringValue.Should().Be("TestClass");
        Payload[nameof(Doc.Comment)].StringValue.Should().Be("summary");
        Payload[nameof(Doc.CommentLength)].IntegerValue.Should().Be(7);
        Payload[nameof(Doc.Code)].StringValue.Should().Be("public class TestClass {}");
        Payload[nameof(Doc.Category)].StringValue.Should().Be("XML");
    }

    [Fact]
    public void ToPayloadDictionary_ShouldHandleNullsGracefully()
    {
        // Arrange
        var Doc = new CodeDocument(
            FileName: "Test.cs",
            ElementType: null!,
            ElementName: "",
            Comment: null!,
            CommentLength: 0,
            Code: null!,
            Category: null!
        );

        // Act
        var Payload = Doc.ToPayloadDictionary;

        // Assert
        Payload[nameof(Doc.ElementType)].StringValue.Should().Be(string.Empty);
        Payload[nameof(Doc.ElementName)].StringValue.Should().Be(string.Empty);
        Payload[nameof(Doc.Comment)].StringValue.Should().Be(string.Empty);
        Payload[nameof(Doc.Category)].StringValue.Should().Be(nameof(QuestionCategory.General));
    }
}
