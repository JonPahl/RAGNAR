using Ragnar.Core.Utils;

namespace RAGNAR.UnitTests.Embedding;

public sealed class PointTests
{

    [Fact]
    public void FromFilePathAndIndex_Throws_WhenPathIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Point.FromFilePathAndIndex(default!, "5".AsSpan()));
    }

    [Fact]
    public void FromFilePathAndIndex_GeneratesDeterministicId()
    {
        const string path = "test.cs";
        const string index = "5";

        var id1 = Point.FromFilePathAndIndex(path.AsSpan(), index.AsSpan());
        var id2 = Point.FromFilePathAndIndex(path.AsSpan(), index.AsSpan());

        Assert.Equal(id1, id2);
        Assert.NotEqual(0UL, id1);
    }

    [Fact]
    public void FromFilePathAndIndex_HandlesLongPaths()
    {
        const string path = "very/long/path/to/a/file/with/many/directories/Program.cs";
        const string index = "42";

        var id = Point.FromFilePathAndIndex(path.AsSpan(), index.AsSpan());

        Assert.NotEqual(0UL, id);
    }

    [Fact]
    public void FromFilePathAndIndex_Throws_WhenIndexIsNull()
    {
        const string? index = null;
        Assert.Throws<ArgumentException>(() => Point.FromFilePathAndIndex("test.cs".AsSpan(), index!.AsSpan()));
    }

    [Fact]
    public void FromFilePathAndIndex_Throws_WhenIndexIsWhitespace()
    {
        const string index = "   ";
        Assert.Throws<ArgumentException>(() => Point.FromFilePathAndIndex("test.cs".AsSpan(), index.AsSpan()));
    }
}
