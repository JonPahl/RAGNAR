namespace RAGNAR.UnitTests;

public class PathExtensionsTests
{
    [Theory]
    [InlineData("file.cs", true)]
    [InlineData("FILE.CS", true)]
    [InlineData("file.txt", false)]
    [InlineData("", false)]
    public void IsExcluded_ReturnsCorrectResult(string FileName, bool Expected)
    {
        // Arrange
        var Exclusions = ImmutableHashSet.Create("file.cs", "Program.cs");
        var Span = FileName.AsSpan();

        // Act
        var Result = Span.IsExcluded(Exclusions);

        // Assert
        Result.Should().Be(Expected);
    }

    [Fact]
    public void IsExcluded_ReturnsFalse_WhenExclusionsNull()
    {
        // Arrange
        var Span = "file.cs".AsSpan();

        // Act
        var Result = Span.IsExcluded(null);

        // Assert
        Result.Should().BeFalse();
    }

    [Fact]
    public void IsExcluded_ReturnsFalse_WhenExclusionsEmpty()
    {
        // Arrange
        var Span = "file.cs".AsSpan();
        var Exclusions = ImmutableHashSet<string>.Empty;

        // Act
        var Result = Span.IsExcluded(Exclusions);

        // Assert
        Result.Should().BeFalse();
    }

    [Fact]
    public void IsExcluded_ThrowsNoException_WhenFileNameEmpty()
    {
        // Arrange
        var Span = "".AsSpan();
        var Exclusions = ImmutableHashSet.Create("file.cs");

        // Act
        var Result = Span.IsExcluded(Exclusions);

        // Assert
        Result.Should().BeFalse();
    }

    [Fact]
    public void ExpandDirectory_ThrowsWhenDirectoryMissing()
    {
        // Arrange
        var NonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        Action Act = () => NonExistent.ExpandDirectory();
        Act.Should().Throw<DirectoryNotFoundException>();
    }

    [Theory]
    [InlineData("file.cs", "FILE.CS", true)]
    [InlineData("file.cs", "other.cs", false)]
    [InlineData("file.cs", null, false)]
    [InlineData("", "file.cs", false)]
    public void IsExcluded_ShouldMatchCaseInsensitive(string FileName, string? Exclusion, bool Expected)
    {
        // Arrange
        var Exclusions = Exclusion is null ? null : ImmutableHashSet.Create(Exclusion);
        var Span = FileName.AsSpan();

        // Act
        var Result = Span.IsExcluded(Exclusions);

        // Assert
        Result.Should().Be(Expected);
    }

    [Theory]
    [InlineData("file.cs", new[] { "FILE.CS", "other.cs" }, true)]
    [InlineData("file.cs", new[] { "other.cs" }, false)]
    public void IsExcluded_ShouldMatchAnyExclusion(string FileName, string[] Exclusions, bool Expected)
    {
        // Arrange
        var Set = Exclusions.ToImmutableHashSet();
        var Span = FileName.AsSpan();

        // Act
        var Result = Span.IsExcluded(Set);

        // Assert
        Result.Should().Be(Expected);
    }
}
