public class UtilsTests
{
    [Fact]
    public void ExpandDirectory_Throws_WhenDirectoryMissing ()
    {
        // Arrange
        var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        var ex = Assert.Throws<DirectoryNotFoundException>(() => nonExistent.ExpandDirectory());
        Assert.Contains(nonExistent, ex.Message);
    }
}
