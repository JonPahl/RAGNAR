namespace Ragnar.IntegrationTests;

public class PathUtilitiesTests
{
    [Fact]
    public void ExpandDirectory_ThrowsWhenDirectoryMissing()
    {
        // Arrange
        var NonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => NonExistent.ExpandDirectory());
    }
}
