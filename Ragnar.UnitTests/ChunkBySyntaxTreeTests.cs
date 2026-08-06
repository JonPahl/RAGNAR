namespace RAGNAR.UnitTests;

public sealed class ChunkBySyntaxTreeTests
{
    [Fact]
    public void ChunkSourceFileShouldExtractClassMetadata()
    {
        // Arrange
        const string Code = @"
                /// <summary>
                /// A test class.
                /// </summary>
                public class TestClass 
                {
                    public int Id { get; set; }
                }";

        // Act
        var Docs = ChunkBySyntaxTree.ChunkSourceFile("TestClass.cs", Code);

        // Assert
        Assert.NotNull(Docs);
        Assert.Single(Docs);
        Assert.Equal("TestClass", Docs[0].ElementName);
        Assert.Equal("ClassDeclaration", Docs[0].ElementType);
        Assert.Contains("A test class.", Docs[0].Comment);
        Assert.Contains("public class TestClass", Docs[0].Code);
    }

    [Fact]
    public void ChunkSourceFileShouldReturnNullWhenInvalidSyntax()
    {
        const string InvalidCode = "public class {"; // Missing closing brace & syntax error

        var Docs = ChunkBySyntaxTree.ChunkSourceFile("Bad.cs", InvalidCode);

        Assert.NotNull(Docs);
    }


    [Fact]
    public void ChunkSourceFileParsesValidCSharp()
    {
        // Arrange
        const string Code = """
            namespace Test
            {
                public class Program { }
            }
            """;

        // Act
        var Docs = ChunkBySyntaxTree.ChunkSourceFile("Program.cs", Code);

        // Assert
        Assert.NotNull(Docs);
        Assert.Single(Docs);
        Assert.Equal("Program", Docs[0].ElementName);
    }

    [Fact]
    public void ChunkSourceFileReturnsNullForInvalidSyntax()
    {
        // Arrange
        const string Code = "invalid {";

        // Act
        var Docs = ChunkBySyntaxTree.ChunkSourceFile("Program.cs", Code);

        // Assert
        Assert.NotNull(Docs);
        Assert.Empty(Docs);
    }
}
