namespace Ragnar.IntegrationTests.Extension;

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
            Category: "Code"
        );

        // Act
        var Dict = Doc.ToPayloadDictionary;

        // Assert
        Assert.Equal("Test.cs", Dict[nameof(CodeDocument.FileName)]);
        Assert.Equal("class", Dict[nameof(CodeDocument.ElementType)]);
        Assert.Equal("TestClass", Dict[nameof(CodeDocument.ElementName)]);
        Assert.Equal("summary", Dict[nameof(CodeDocument.Comment)]);
        Assert.Equal(7, Dict[nameof(CodeDocument.CommentLength)]);
        Assert.Equal("public class TestClass {}", Dict[nameof(CodeDocument.Code)]);
        Assert.Equal("Code", Dict[nameof(CodeDocument.Category)]);
    }

    [Fact]
    public void ToPayloadDictionary_HandlesNulls()
    {
        // Arrange
        var Doc = new CodeDocument
        (
            FileName: "Test.cs",
            ElementType: "",
            ElementName: "",
            Comment: "",
            CommentLength: 0,
            Code: "");

        // Act
        var Dict = Doc.ToPayloadDictionary;

        // Assert
        Assert.Equal(string.Empty, Dict[nameof(CodeDocument.ElementType)].StringValue);
        Assert.Equal(string.Empty, Dict[nameof(CodeDocument.ElementName)].StringValue);
        Assert.Equal(string.Empty, Dict[nameof(CodeDocument.Comment)].StringValue);
        Assert.Equal(nameof(QuestionCategory.Other), Dict[nameof(CodeDocument.Category)].StringValue);
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
            Category: "General"
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
    public void ToPayloadDictionary_HandlesNullsAndEmptyStrings()
    {
        // Arrange
        var Doc = new CodeDocument
        (
            FileName: "Test.cs",
            ElementType: "",
            ElementName: "",
            Comment: "",
            CommentLength: 0,
            Code: "");

        // Act
        var Dict = Doc.ToPayloadDictionary;

        // Assert
        Dict[nameof(CodeDocument.ElementType)].StringValue.Should().Be(string.Empty);
        Dict[nameof(CodeDocument.ElementName)].StringValue.Should().Be(string.Empty);
        Dict[nameof(CodeDocument.Comment)].StringValue.Should().Be(string.Empty);
        Dict[nameof(CodeDocument.Category)].StringValue.Should().Be(nameof(QuestionCategory.Other));
    }

    [Fact]
    public void ToPayloadDictionaryUsesDefaultCategoryWhenNull()
    {
        // Arrange
        var Doc = new CodeDocument
        (
            FileName: "Test.cs",
            ElementType: "",
            ElementName: "",
            Comment: "",
            CommentLength: 0,
            Code: "",
            Category: null!
        );

        // Act
        var Dict = Doc.ToPayloadDictionary;

        // Assert
        Dict[nameof(CodeDocument.Category)].StringValue.Should().Be(nameof(QuestionCategory.General));
    }

    [Fact]
    public void ToPayloadDictionary_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var Doc = new CodeDocument
        (
            FileName: "Program.cs",
            ElementType: "class",
            ElementName: "Program",
            Comment: "Main entry point.",
            CommentLength: 16,
            Code: "public static void Main() { }",
            Category: QuestionCategory.XML.ToString()
        );

        // Act
        var Payload = Doc.ToPayloadDictionary;

        // Assert
        Assert.Equal(7, Payload.Count);
        Assert.Equal("Program.cs", Payload[nameof(Doc.FileName)].StringValue);
        Assert.Equal("class", Payload[nameof(Doc.ElementType)].StringValue);
        Assert.Equal("Program", Payload[nameof(Doc.ElementName)].StringValue);
        Assert.Equal("Main entry point.", Payload[nameof(Doc.Comment)].StringValue);
        Assert.Equal(16, Payload[nameof(Doc.CommentLength)].IntegerValue);
        Assert.Equal("public static void Main() { }", Payload[nameof(Doc.Code)].StringValue);
        Assert.Equal(QuestionCategory.XML.ToString(), Payload[nameof(Doc.Category)].StringValue);
    }

    [Fact]
    public void ToPayloadDictionary_ShouldHandleNulls()
    {
        // Arrange
        var Doc = new CodeDocument
        (
            FileName: null,
            ElementType: null,
            ElementName: null,
            Comment: null,
            CommentLength: 0,
            Code: null,
            Category: null);

        // Act
        var Payload = Doc.ToPayloadDictionary;

        // Assert
        Assert.Equal(string.Empty, Payload[nameof(Doc.FileName)].StringValue);
        Assert.Equal(string.Empty, Payload[nameof(Doc.ElementType)].StringValue);
        Assert.Equal(string.Empty, Payload[nameof(Doc.ElementName)].StringValue);
        Assert.Equal(string.Empty, Payload[nameof(Doc.Comment)].StringValue);
        Assert.Equal(QuestionCategory.General.ToString(), Payload[nameof(Doc.Category)].StringValue);
    }
}
