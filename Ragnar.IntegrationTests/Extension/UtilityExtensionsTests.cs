namespace Ragnar.IntegrationTests;

public class UtilityExtensionsTests
{
    [Fact]
    public void ExpandDirectory_Should_Expand_Env_Var()
    {
        Environment.SetEnvironmentVariable("MY_VAR", "C:\\temp");
        var result = "%MY_VAR%".ExpandDirectory();
        Assert.Equal("C:\\temp", result);
    }

    [Fact]
    public void ExpandDirectory_Should_Throw_If_Directory_Not_Found()
    {
        Assert.Throws<DirectoryNotFoundException>(() => "C:\\NonExistentDir12345".ExpandDirectory());
    }
}
