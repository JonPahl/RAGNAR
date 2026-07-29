namespace Ragnar.IntegrationTests;

public class PathExtensionsTests
{
    [Theory]
    [InlineData("file.cs", new[] { "FILE.CS" }, true)]
    [InlineData("file.cs", new[] { "other.cs" }, false)]
    public void IsExcluded_CaseInsensitive (string fileName, string[] exclusions, bool expected)
    {
        // Act
        var result = fileName.AsSpan().IsExcluded(exclusions);
        // Assert
        Assert.Equal(expected, result);
    }
}
