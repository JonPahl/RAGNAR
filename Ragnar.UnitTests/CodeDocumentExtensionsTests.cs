namespace RAGNAR.UnitTests;

public class CodeDocumentExtensionsTests
{

    [Fact]
    public void ToPayloadDictionary_IncludesAllFields()
    {
        // Arrange
        var Doc = new CodeDocument
        (
            FileName: "Test.cs",
            ElementType: "class",
            ElementName: "TestClass",
            Comment: "summary",
            CommentLength: 7,
            Code: "public class TestClass {}",
            Category: nameof(QuestionCategory.General)
        );

        // Act
        var Dict = Doc.ToPayloadDictionary;

        // Assert
        Dict.Should().NotBeNull();
        Dict.Should().HaveCount(7);
        Dict[nameof(Doc.FileName)].StringValue.Should().Be("Test.cs");
        Dict[nameof(Doc.ElementType)].StringValue.Should().Be("class");
        Dict[nameof(Doc.ElementName)].StringValue.Should().Be("TestClass");
        Dict[nameof(Doc.Comment)].StringValue.Should().Be("summary");
        Dict[nameof(Doc.Code)].StringValue.Should().Be("public class TestClass {}");
        Dict[nameof(Doc.Category)].StringValue.Should().Be("General");
    }

    [Fact]
    public void ToPayloadDictionary_HandlesNulls()
    {
        // Arrange
        var Doc = new CodeDocument
        (
            FileName: "Test.cs",
            ElementType: null,
            ElementName: null,
            Comment: null,
            CommentLength: 0,
            Code: null,
            Category: null
        );

        // Act
        var Dict = Doc.ToPayloadDictionary;

        // Assert
        Dict[nameof(Doc.ElementType)].StringValue.Should().Be(string.Empty);
        Dict[nameof(Doc.ElementName)].StringValue.Should().Be(string.Empty);
        Dict[nameof(Doc.Comment)].StringValue.Should().Be(string.Empty);
        Dict[nameof(Doc.Category)].StringValue.Should().Be(nameof(QuestionCategory.General));
    }

    [Fact]
    public void ToPayloadDictionary_ReturnsNonEmptyDictionary()
    {
        // Arrange
        var Doc = new CodeDocument
        (
            FileName: "Program.cs",
            ElementType: "class",
            ElementName: "Program",
            Comment: "Main entry point",
            CommentLength: 18,
            Code: "public static void Main() { }",
            Category: nameof(QuestionCategory.General)
        );

        // Act
        var Payload = Doc.ToPayloadDictionary;

        // Assert
        Payload.Should().NotBeNull().And.HaveCount(7);
        Payload.Keys.Should().Contain(
        [
            nameof(Doc.FileName),
            nameof(Doc.ElementType),
            nameof(Doc.ElementName),
            nameof(Doc.Comment),
            nameof(Doc.CommentLength),
            nameof(Doc.Code),
            nameof(Doc.Category)
        ]);
    }

    [Fact]
    public void ToPayloadDictionary_ShouldMapAllProperties()
    {
        // Arrange
        var Doc = new CodeDocument
        (
            FileName: "Program.cs",
            ElementType: "class",
            ElementName: "Program",
            Comment: "Main entry point.",
            CommentLength: 17,
            Code: "public static void Main() { }",
            Category:
            nameof(QuestionCategory.XML)
        );

        // Act
        var Payload = Doc.ToPayloadDictionary;

        // Assert
        Payload.Should().HaveCount(7);
        Payload[nameof(Doc.FileName)].StringValue.Should().Be("Program.cs");
        Payload[nameof(Doc.ElementType)].StringValue.Should().Be("class");
        Payload[nameof(Doc.ElementName)].StringValue.Should().Be("Program");
        Payload[nameof(Doc.Comment)].StringValue.Should().Be("Main entry point.");
        Payload[nameof(Doc.CommentLength)].IntegerValue.Should().Be(17);
        Payload[nameof(Doc.Code)].StringValue.Should().Be("public static void Main() { }");
        Payload[nameof(Doc.Category)].StringValue.Should().Be("XML");
    }

    [Fact]
    public void ToPayloadDictionary_ShouldHandleNulls()
    {
        // Arrange
        var Doc = new CodeDocument(

            FileName: string.Empty,
            ElementType: "",
            ElementName: "",
            Comment: "",
            CommentLength: 5,
            Code: "");

        // Act
        var Payload = Doc.ToPayloadDictionary;

        // Assert
        Payload[nameof(Doc.FileName)].StringValue.Should().Be(string.Empty);
        Payload[nameof(Doc.ElementType)].StringValue.Should().Be(string.Empty);
        Payload[nameof(Doc.ElementName)].StringValue.Should().Be(string.Empty);
        Payload[nameof(Doc.Comment)].StringValue.Should().Be(string.Empty);
        Payload[nameof(Doc.Code)].StringValue.Should().Be(string.Empty);
        Payload[nameof(Doc.Category)].StringValue.Should().Be(nameof(QuestionCategory.General));
    }
}
