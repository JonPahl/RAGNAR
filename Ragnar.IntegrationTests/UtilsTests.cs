namespace Ragnar.IntegrationTests;

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
}
