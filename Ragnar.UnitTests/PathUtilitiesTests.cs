namespace RAGNAR.UnitTests;

public class PathUtilitiesTests
{
    [Fact]
    public void ExpandDirectory_ThrowsWhenDirectoryMissing()
    {
        // Arrange
        var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        Action Act = () => nonExistent.ExpandDirectory();
        Act.Should().Throw<DirectoryNotFoundException>();
    }
}
