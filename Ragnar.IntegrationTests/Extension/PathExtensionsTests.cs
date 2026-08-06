namespace Ragnar.IntegrationTests.Extension;

public class PathExtensionsTests
{
    [Theory]
    [InlineData("file.cs", new[] { "FILE.CS" }, true)]
    [InlineData("Program.cs", new[] { "file.cs" }, false)]
    [InlineData("test.cs", null, false)]
    [InlineData("", new[] { "file.cs" }, false)]
    public void IsExcluded_ShouldMatchCaseInsensitively(string FileName, string[]? Exclusions, bool Expected)
    {
        // Arrange
        var ExclusionSet = Exclusions?.ToImmutableHashSet() ?? [];

        // Act
        var Result = FileName.AsSpan().IsExcluded(ExclusionSet);

        // Assert
        Result.Should().Be(Expected);
    }

    [Theory]
    [InlineData("file.cs", new string[] { }, false)]
    [InlineData("file.cs", new[] { "FILE.CS" }, true)]
    [InlineData("file.cs", new[] { "other.cs" }, false)]
    [InlineData("", new[] { "file.cs" }, false)]
    [InlineData("file.cs", null, false)]
    public void IsExcluded_ShouldMatchCaseInsensitive(string Filename, string[]? Exclusions, bool Expected)
    {
        // Arrange
        var ExclusionSet = Exclusions is null ? null : ImmutableHashSet.CreateRange(Exclusions);

        // Act
        var Result = Filename.AsSpan().IsExcluded(ExclusionSet);

        // Assert
        Assert.Equal(Expected, Result);
    }

    [Theory]
    [InlineData("file.cs", new[] { "FILE.CS" }, true)]
    [InlineData("file.cs", new[] { "other.cs" }, false)]
    [InlineData("", new[] { "file.cs" }, false)]
    public void IsExcluded_Should_Handle_Case_Insensitive(string fileName, string[] exclusions, bool expected)
    {
        var result = fileName.AsSpan().IsExcluded(ImmutableHashSet.CreateRange(exclusions));
        Assert.Equal(expected, result);
    }
}
