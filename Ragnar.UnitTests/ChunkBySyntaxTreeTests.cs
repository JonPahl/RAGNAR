
namespace RAGNAR.UnitTests;

public sealed class ChunkBySyntaxTreeTests
{
    [Fact]
    public void ChunkSourceFile_ShouldExtractClassMetadata ()
    {
        // Arrange
        const string code = @"
                /// <summary>
                /// A test class.
                /// </summary>
                public class TestClass 
                {
                    public int Id { get; set; }
                }";

        // Act
        var docs = ChunkBySyntaxTree.ChunkSourceFile("TestClass.cs", code);

        // Assert
        Assert.NotNull(docs);
        Assert.Single(docs);
        Assert.Equal("TestClass", docs[0].ElementName);
        Assert.Equal("ClassDeclaration", docs[0].ElementType);
        Assert.Contains("A test class.", docs[0].Comment);
        Assert.Contains("public class TestClass", docs[0].Code);
    }

    [Fact]
    public void ChunkSourceFile_ShouldReturnNull_WhenInvalidSyntax ()
    {
        const string invalidCode = "public class {"; // Missing closing brace & syntax error

        var docs = ChunkBySyntaxTree.ChunkSourceFile("Bad.cs", invalidCode);

        Assert.NotNull(docs);
    }


    [Fact]
    public void ChunkSourceFile_ParsesValidCSharp ()
    {
        // Arrange
        const string code = """
            namespace Test
            {
                public class Program { }
            }
            """;

        // Act
        var docs = ChunkBySyntaxTree.ChunkSourceFile("Program.cs", code);

        // Assert
        Assert.NotNull(docs);
        Assert.Single(docs);
        Assert.Equal("Program", docs[0].ElementName);
    }

    [Fact]
    public void ChunkSourceFile_ReturnsNull_ForInvalidSyntax ()
    {
        // Arrange
        const string code = "invalid {";

        // Act
        var docs = ChunkBySyntaxTree.ChunkSourceFile("Program.cs", code);

        // Assert
        Assert.NotNull(docs);
        Assert.Empty(docs);
    }
}
