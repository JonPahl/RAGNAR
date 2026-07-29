namespace Ragnar.UnitTests.Extensions;

public class CodeDocumentExtensionsTests
{
    [Fact]
    public void ToDictionary_IncludesAllFields ()
    {
        // Arrange
        var doc = new CodeDocument
        {
            FileName = "Test.cs",
            ElementType = "class",
            ElementName = "TestClass",
            Comment = "summary",
            Comment_Length = 7,
            Code = "public class TestClass {}",
            Category = "Code"
        };

        // Act
        var dict = doc.ToDictionary;

        // Assert
        Assert.Equal("Test.cs", dict[nameof(CodeDocument.FileName)]);
        Assert.Equal("class", dict[nameof(CodeDocument.ElementType)]);
        Assert.Equal("TestClass", dict[nameof(CodeDocument.ElementName)]);
        Assert.Equal("summary", dict[nameof(CodeDocument.Comment)]);
        Assert.Equal(7, dict[nameof(CodeDocument.Comment_Length)]);
        Assert.Equal("public class TestClass {}", dict[nameof(CodeDocument.Code)]);
        Assert.Equal("Code", dict[nameof(CodeDocument.Category)]);
    }

    [Fact]
    public void ToDictionary_HandlesNulls ()
    {
        // Arrange
        var doc = new CodeDocument { FileName = "Test.cs", Code = "", Comment = "", Comment_Length = 0, ElementName = "", ElementType = "" };

        // Act
        var dict = doc.ToDictionary;

        // Assert
        Assert.Equal(string.Empty, dict[nameof(CodeDocument.ElementType)]);
        Assert.Equal(string.Empty, dict[nameof(CodeDocument.ElementName)]);
        Assert.Equal(string.Empty, dict[nameof(CodeDocument.Comment)]);
        Assert.Equal("General", dict[nameof(CodeDocument.Category)]);
    }
}
