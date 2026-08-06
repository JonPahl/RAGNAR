namespace Ragnar.UnitTests;

public class UtilsTests
{

    [Fact]
    public void ExpandDirectoryReturnsFullPathWhenDirectoryExists()
    {
        // Arrange
        var TempDir = Path.GetTempPath();
        Directory.CreateDirectory(TempDir); // Ensure exists

        // Act
        var Expanded = TempDir.ExpandDirectory();

        // Assert
        Expanded.Should().Be(Path.GetFullPath(TempDir));
    }

    [Fact]
    public void ExpandDirectoryThrowsWhenDirectoryMissing()
    {
        // Arrange
        var NonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => NonExistent.ExpandDirectory());
    }

    [Fact]
    public void ExpandDirectoryExpandsEnvironmentVariables()
    {
        // Arrange
        var Path = "%TEMP%\\";

        // Act
        var Expanded = Path.ExpandDirectory();

        // Assert
        Assert.NotNull(Expanded);
        Assert.Contains(System.IO.Path.GetTempPath(), Expanded);
    }
}