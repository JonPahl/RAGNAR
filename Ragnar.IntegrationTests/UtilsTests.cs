namespace Ragnar.IntegrationTests;

public class UtilsTests
{
    //[Fact]
    //public void ExpandDirectory_Throws_WhenDirectoryMissing ()
    //{
    //    // Arrange
    //    var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    //    // Act & Assert
    //    Assert.Throws<DirectoryNotFoundException>(() => nonExistent.ExpandDirectory());
    //}

    [Fact]
    public void ExpandDirectory_ExpandsEnvironmentVariables ()
    {
        // Arrange
        var path = "%TEMP%\\subdir";
        var expected = Path.Combine(Path.GetTempPath(), "subdir");

        // Act
        var expanded = path.ExpandDirectory();

        // Assert
        Assert.Equal(expected, expanded);
    }

    [Fact]
    public void ExpandDirectory_Throws_WhenPathNull ()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((string?)null).ExpandDirectory());
    }
}
