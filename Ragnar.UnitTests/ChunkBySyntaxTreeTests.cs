using Ragnar.Embedding.Chunker;

namespace RAGNAR.UnitTests;

public sealed class ChunkBySyntaxTreeTests
{
    [Fact]
    public void ChunkSourceFile_ParsesValidCSharp()
    {
        // Arrange
        var chunker = new ChunkBySyntaxTree();
        var code = """
            namespace Test
            {
                public class Program { }
            }
            """;

        // Act
        var docs = chunker.ChunkSourceFile("Program.cs", code);

        // Assert
        Assert.NotNull(docs);
        Assert.Single(docs);
        Assert.Equal("Program", docs[0].ElementName);
    }

    [Fact]
    public void ChunkSourceFile_ReturnsNull_ForInvalidSyntax()
    {
        // Arrange
        var chunker = new ChunkBySyntaxTree();
        var code = "invalid {";

        // Act
        var docs = chunker.ChunkSourceFile("Program.cs", code);

        // Assert
        Assert.Empty(docs);
    }
}
