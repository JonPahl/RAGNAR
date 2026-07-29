namespace Ragnar.UnitTests.Extensions;

public class PathExtensionsTests
{
    [Theory]
    [InlineData("file.cs", new[] { "FILE.CS" }, true)]
    [InlineData("file.cs", new[] { "other.cs" }, false)]
    // [InlineData("test.cs", Array.Empty<string>(), false)]
    public void IsExcluded_CaseInsensitive (string fileName, string[] exclusions, bool expected)
    {
        // Act
        var result = fileName.AsSpan().IsExcluded(exclusions);

        // Assert
        Assert.Equal(expected, result);
    }
}
