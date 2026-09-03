using Ragnar.Core.Model;

namespace RAGNAR.UnitTests;

public sealed class CodeDocumentExtensionsTests
{
    [Fact]
    public void Dictionary_ReturnsNonEmptyDictionary()
    {
        // Arrange
        var doc = new CodeDocument
        {
            FileName = "Program.cs",
            ElementType = "Class",
            ElementName = "Program",
            Comment = "// Main",
            Comment_Length = 6,
            Code = "public class Program { }",
            Category = "Refactor"
        };

        // Act
        var dict = doc.Dictionary;

        // Assert
        Assert.Equal(7, dict.Count);
        Assert.Equal("Program.cs", dict[nameof(CodeDocument.FileName)]);
        Assert.Equal("Refactor", dict[nameof(CodeDocument.Category)]);
    }
}
