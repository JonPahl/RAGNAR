using Ragnar.Extensions;

namespace RAGNAR.UnitTests;

public sealed class PathExtensionsTests
{
    [Theory]
    [InlineData("file.txt", new[] { "FILE.TXT", "other.txt" }, true)]
    [InlineData("file.txt", new[] { "other.txt" }, false)]
    [InlineData("File.TXT", new[] { "file.txt" }, true)]
    public void IsExcluded_ReturnsExpected(string fileName, string[] exclusions, bool expected)
    {
        // Act
        var result = fileName.AsSpan().IsExcluded(exclusions);

        // Assert
        Assert.Equal(expected, result);
    }
}
