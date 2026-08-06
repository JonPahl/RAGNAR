namespace Ragnar.UnitTests.Extensions;

public class CodeDocumentExtensionsTests
{
    [Fact]
    public void ToPayloadDictionary_IncludesAllFields()
    {
        // Arrange
        var Doc = new CodeDocument
        (
            "Test.cs",
            "class",
            "TestClass",
            "summary",
            7,
            "public class TestClass {}",
            nameof(QuestionCategory.Other)
        );

        // Act
        var Dict = Doc.ToPayloadDictionary;

        // Assert
        Dict.Should().HaveCount(7)
            .And.ContainKey(nameof(CodeDocument.FileName))
            .And.ContainKey(nameof(CodeDocument.ElementType))
            .And.ContainKey(nameof(CodeDocument.ElementName))
            .And.ContainKey(nameof(CodeDocument.Comment))
            .And.ContainKey(nameof(CodeDocument.CommentLength))
            .And.ContainKey(nameof(CodeDocument.Code))
            .And.ContainKey(nameof(CodeDocument.Category));

        Dict[nameof(CodeDocument.FileName)].StringValue.Should().Be("Test.cs");
        Dict[nameof(CodeDocument.ElementType)].StringValue.Should().Be("class");
        Dict[nameof(CodeDocument.ElementName)].StringValue.Should().Be("TestClass");
        Dict[nameof(CodeDocument.Comment)].StringValue.Should().Be("summary");
        Dict[nameof(CodeDocument.CommentLength)].IntegerValue.Should().Be(7);
        Dict[nameof(CodeDocument.Code)].StringValue.Should().Be("public class TestClass {}");
        Dict[nameof(CodeDocument.Category)].StringValue.Should().Be("Other");
    }

    [Fact]
    public void ToPayloadDictionary_HandlesNulls()
    {
        // Arrange
        var CodeDocument = new CodeDocument
            (
            FileName: null,
            ElementType: null,
            ElementName: null,
            Comment: null,
            CommentLength: 0,
            Code: null,
            Category: nameof(QuestionCategory.General));

        // Act
        var Dict = CodeDocument.ToPayloadDictionary;

        // Assert
        Dict[nameof(CodeDocument.ElementType)].StringValue.Should().Be(string.Empty);
        Dict[nameof(CodeDocument.ElementName)].StringValue.Should().Be(string.Empty);
        Dict[nameof(CodeDocument.Comment)].StringValue.Should().Be(string.Empty);
        Dict[nameof(CodeDocument.Category)].StringValue.Should().Be(nameof(QuestionCategory.General));
    }

    [Fact]
    public void ToPayloadDictionary_ReturnsNonEmptyDictionary()
    {
        // Arrange
        var Doc = new CodeDocument(
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
        Payload.Should().NotBeNull()
            .And.HaveCount(7)
            .And.ContainKey(nameof(CodeDocument.FileName))
            .And.ContainKey(nameof(CodeDocument.ElementType))
            .And.ContainKey(nameof(CodeDocument.ElementName))
            .And.ContainKey(nameof(CodeDocument.Comment))
            .And.ContainKey(nameof(CodeDocument.CommentLength))
            .And.ContainKey(nameof(CodeDocument.Code))
            .And.ContainKey(nameof(CodeDocument.Category));
    }

    [Fact]
    public void ToPayloadDictionaryShouldMapAllPropertiesCorrectly()
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
            Category: nameof(QuestionCategory.XML)
        );

        // Act
        var Payload = Doc.ToPayloadDictionary;

        // Assert
        Assert.Equal(7, Payload.Count);
        Assert.Equal("Program.cs", Payload[nameof(Doc.FileName)].StringValue);
        Assert.Equal("class", Payload[nameof(Doc.ElementType)].StringValue);
        Assert.Equal("Program", Payload[nameof(Doc.ElementName)].StringValue);
        Assert.Equal("Main entry point.", Payload[nameof(Doc.Comment)].StringValue);
        Assert.Equal(17, Payload[nameof(Doc.CommentLength)].IntegerValue);
        Assert.Equal("public static void Main() { }", Payload[nameof(Doc.Code)].StringValue);
        Assert.Equal(nameof(QuestionCategory.XML), Payload[nameof(Doc.Category)].StringValue);
    }

    [Fact]
    public void ToPayloadDictionary_ShouldHandleNulls()
    {
        // Arrange
        var Doc = new CodeDocument(
            FileName: "",
            ElementType: "",
            ElementName: "",
            Comment: "",
            CommentLength: 0,
            Code: "",
            Category: nameof(QuestionCategory.General)
        );

        // Act
        var Payload = Doc.ToPayloadDictionary;

        // Assert
        Assert.Equal(string.Empty, Payload[nameof(Doc.FileName)].StringValue);
        Assert.Equal(string.Empty, Payload[nameof(Doc.ElementType)].StringValue);
        Assert.Equal(string.Empty, Payload[nameof(Doc.ElementName)].StringValue);
        Assert.Equal(string.Empty, Payload[nameof(Doc.Comment)].StringValue);
        Assert.Equal(0, Payload[nameof(Doc.CommentLength)].IntegerValue);
        Assert.Equal(string.Empty, Payload[nameof(Doc.Code)].StringValue);
        Assert.Equal(nameof(QuestionCategory.General), Payload[nameof(Doc.Category)].StringValue);
    }
}
