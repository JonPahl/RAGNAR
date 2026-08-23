namespace Ragnar.Tests;

public class UtilsTests
{
    [Fact]
    public void ExpandDirectory_Expands_Env_Var_And_Throws_If_Missing()
    {
        // Arrange
        var path = "%NONEXISTENT_ENV_VAR%\\folder";

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => path.ExpandDirectory());
    }

}