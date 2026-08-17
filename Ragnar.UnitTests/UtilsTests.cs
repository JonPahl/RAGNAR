namespace Ragnar.UnitTests;

public sealed class UtilsTests
{
    [Fact]
    public void ExpandDirectory_Throws_WhenDirectoryMissing()
    {
        // Arrange
        var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => nonExistent.ExpandDirectory());
    }

    [Fact]

    public void ExpandDirectory_ReturnsFullPath_WhenDirectoryExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = tempDir.ExpandDirectory();
            result.Should().Be(Path.GetFullPath(tempDir));
        }
        finally
        {
            if(Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ExpandDirectory_ThrowsDirectoryNotFoundException_WhenPathDoesNotExist()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Assert.Throws<DirectoryNotFoundException>(() => nonExistentPath.ExpandDirectory());
    }
}
