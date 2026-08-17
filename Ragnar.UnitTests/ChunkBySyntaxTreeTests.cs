namespace Ragnar.UnitTests;

public sealed class ChunkBySyntaxTreeTests
{
    private readonly ChunkBySyntaxTree Chunker = new();

    [Fact]
    public void ChunkSourceFile_InferCorrectCategory_FromPath()
    {
        const string Code = "public class TestClass { }";

        var testResult = Chunker.ChunkSourceFile("src/Testing/TestFile.cs", Code);
        testResult[0].Category.Should().Be("Testing");

        var pluginResult = Chunker.ChunkSourceFile("plugins/MyPlugin.cs", Code);
        pluginResult[0].Category.Should().Be("Plugin");
    }

    [Fact]
    public void ChunkSourceFile_ParsesValidCSharp()
    {
        // Arrange
        var chunker = new ChunkBySyntaxTree();
        const string code = """
            namespace Test
            {
                public class Program { }
            }
            """;

        // Act
        var docs = chunker.ChunkSourceFile("Program.cs", code);

        // Assert
        docs.Should().NotBeNull();
        Assert.Single(docs);
        docs[0].ElementName.Should().Be("Program");
    }

    [Fact]
    public void ChunkSourceFile_ReturnsNull_ForInvalidSyntax()
    {
        // Arrange
        var chunker = new ChunkBySyntaxTree();
        const string Code = "invalid {";

        // Act
        var docs = chunker.ChunkSourceFile("Program.cs", Code);

        // Assert
        docs.Should().BeEmpty();
    }

    [Fact]
    public void ChunkSourceFile_ReturnsCodeDocuments_WhenValidClassExists()
    {
        const string code = @"
            namespace TestNamespace
            {
                /// <summary>Test class</summary>
                public class TestClass
                {
                    public void TestMethod() {}
                }
            }";

        var chunker = new ChunkBySyntaxTree();
        var result = chunker.ChunkSourceFile("TestClass.cs", code);

        result.Should().NotBeNull();
        Assert.Single(result);
        result.First().ElementName.Should().Be("TestClass");
        Assert.Contains("Test class", result.First().Comment, StringComparison.OrdinalIgnoreCase);
    }
}
