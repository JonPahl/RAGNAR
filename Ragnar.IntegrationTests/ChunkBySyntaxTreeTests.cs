namespace Ragnar.IntegrationTests;

public class ChunkBySyntaxTreeTests
{
    [Fact]
    public void ChunkSourceFile_Should_ExtractTopLevelClasses ()
    {
        // Arrange
        var code = """
            using System;

            namespace MyApp
            {
                public class Program
                {
                    public static void Main() { }
                }

                public class Helper
                {
                    public int Add(int a, int b) => a + b;
                }
            }
            """;

        // Act
        var docs = ChunkBySyntaxTree.ChunkSourceFile("Program.cs", code);

        // Assert
        Assert.NotNull(docs);
        Assert.Equal(2, docs.Count); // Program + Helper
        Assert.Contains(docs, d => d.ElementName == "Program");
        Assert.Contains(docs, d => d.ElementName == "Helper");
    }

    //[Fact]
    //public void ChunkSourceFile_ReturnsNull_OnInvalidSyntax ()
    //{
    //    var invalidCode = "class { }"; // missing name
    //    var docs = ChunkBySyntaxTree.ChunkSourceFile("bad.cs", invalidCode);
    //    Assert.Null(docs);
    //}
}

