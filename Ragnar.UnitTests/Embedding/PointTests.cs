namespace Ragnar.Tests.Embedding;

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
        const string PATH = "test.cs";
        const string INDEX = "5";

        var id1 = Point.FromFilePathAndIndex(PATH.AsSpan(), INDEX.AsSpan());
        var id2 = Point.FromFilePathAndIndex(PATH.AsSpan(), INDEX.AsSpan());

        Assert.Equal(id1, id2);
        Assert.NotEqual(0UL, id1);
    }

    [Fact]
    public void FromFilePathAndIndex_HandlesLongPaths()
    {
        const string PATH = "very/long/path/to/a/file/with/many/directories/Program.cs";
        const string INDEX = "42";

        var id = Point.FromFilePathAndIndex(PATH.AsSpan(), INDEX.AsSpan());

        Assert.NotEqual(0UL, id);
    }

    [Fact]
    public void FromFilePathAndIndex_Throws_WhenIndexIsNull()
    {
        const string? INDEX = null;
        Assert.Throws<ArgumentException>(() => Point.FromFilePathAndIndex("test.cs".AsSpan(), INDEX!.AsSpan()));
    }

    [Fact]
    public void FromFilePathAndIndex_Throws_WhenIndexIsWhitespace()
    {
        const string INDEX = "   ";
        Assert.Throws<ArgumentException>(() => Point.FromFilePathAndIndex("test.cs".AsSpan(), INDEX.AsSpan()));
    }
}
