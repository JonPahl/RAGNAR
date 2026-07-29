namespace Ragnar.IntegrationTests;

public class CodeDocumentExtensionsTests
{
    [Fact]
    public void ToDictionary_ReturnsAllFields ()
    {
        // Arrange
        var doc = new CodeDocument
        {
            FileName = "test.cs",
            ElementType = "Class",
            ElementName = "TestClass",
            Comment = "Summary comment",
            Comment_Length = 15,
            Code = "public class TestClass { }",
            Category = nameof(QuestionCategory.General),
        };

        // Act
        var dict = doc.ToDictionary;

        // Assert
        Assert.Equal(7, dict.Count);
        Assert.Equal("test.cs", dict[nameof(CodeDocument.FileName)]);
        Assert.Equal("Class", dict[nameof(CodeDocument.ElementType)]);
        Assert.Equal("TestClass", dict[nameof(CodeDocument.ElementName)]);
        Assert.Equal("Summary comment", dict[nameof(CodeDocument.Comment)]);
        Assert.Equal(15, dict[nameof(CodeDocument.Comment_Length)]);
        Assert.Equal("public class TestClass { }", dict[nameof(CodeDocument.Code)]);
        Assert.Equal("General", dict[nameof(CodeDocument.Category)]);
    }

    [Fact]
    public void ToDictionary_HandlesNulls ()
    {
        // Arrange
        var doc = new CodeDocument
        {
            FileName = "test.cs",
            ElementType = null,
            ElementName = null,
            Comment = null,
            Code = null,
            Category = null,
            Comment_Length = 0,
        };

        // Act
        var dict = doc.ToDictionary;

        // Assert
        Assert.Equal(string.Empty, dict[nameof(CodeDocument.ElementType)]);
        Assert.Equal(string.Empty, dict[nameof(CodeDocument.ElementName)]);
        Assert.Equal(string.Empty, dict[nameof(CodeDocument.Comment)]);
        Assert.Equal(string.Empty, dict[nameof(CodeDocument.Code)]);
        Assert.Equal("General", dict[nameof(CodeDocument.Category)]);
    }
}
