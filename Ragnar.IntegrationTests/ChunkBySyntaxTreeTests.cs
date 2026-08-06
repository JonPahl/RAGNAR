namespace Ragnar.IntegrationTests;

public class ChunkBySyntaxTreeTests
{
    [Fact]
    public void ChunkSourceFile_Should_Return_Null_For_Invalid_Syntax()
    {
        var docs = ChunkBySyntaxTree.ChunkSourceFile("Bad.cs", "not valid c# {");
        Assert.Null(docs); // or empty list? depends on impl.
    }

    [Fact]
    public void ChunkSourceFile_Should_Parse_Simple_Class()
    {
        var code = """
            public class Foo { }
            """;
        var docs = ChunkBySyntaxTree.ChunkSourceFile("Foo.cs", code);

        Assert.NotNull(docs);
        Assert.Single(docs);
        Assert.Equal("Foo", docs[0].ElementName);
    }
}

