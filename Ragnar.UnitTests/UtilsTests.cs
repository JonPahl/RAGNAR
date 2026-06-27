using Ragnar.Utils;

namespace RAGNAR.UnitTests;

public sealed class UtilsTests
{
    //[Theory]
    //[InlineData("%TEMP%", typeof(Environment).AssemblyQualifiedName)] // placeholder
    //public void ExpandDirectory_ExpandsEnvVars(string path, string expectedContains)
    //{
    //    // Act
    //    var expanded = path.ExpandDirectory();

    //    // Assert
    //    Assert.NotNull(expanded);
    //    Assert.NotEmpty(expanded);
    //}

    [Fact]
    public void ExpandDirectory_Throws_WhenDirectoryMissing()
    {
        // Arrange
        var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => nonExistent.ExpandDirectory());
    }
}
