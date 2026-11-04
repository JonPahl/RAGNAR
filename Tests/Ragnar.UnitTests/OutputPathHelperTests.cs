namespace Ragnar.Tests;

public class OutputPathHelperTests
{
    [Fact]
    public void GetResponseDirectoryWithEmptyFoldersAndEmptyBaseShouldReturnDefault()
    {
        var result = ((IEnumerable<string>)[]).GetResponseDirectory();

        Assert.Equal(DomainDefaults.ResponseDir, result);
    }

    [Fact]
    public void GetResponseDirectoryWithEmptyFoldersAndBaseDirShouldCombine()
    {
        var result = ((IEnumerable<string>)[]).GetResponseDirectory("/base");

        Assert.Equal(Path.Join("/base", DomainDefaults.ResponseDir), result);
    }

    [Fact]
    public void GetResponseDirectoryWithFoldersShouldAppendDefaultDir()
    {
        var folders = new[] { "category", "sub" };

        var result = folders.GetResponseDirectory();

        Assert.Equal(Path.Join("category", "sub", DomainDefaults.ResponseDir), result);
    }

    [Fact]
    public void GetResponseDirectoryWithFoldersAndBaseDirShouldIgnoreBaseDir()
    {
        var folders = new[] { "category", "sub" };

        var result = folders.GetResponseDirectory("base");

        Assert.Equal(Path.Join("base", "category", "sub", DomainDefaults.ResponseDir), result);
    }

    [Fact]
    public void GetResponseDirectoryWithNullFoldersShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => ((IEnumerable<string>)null!).GetResponseDirectory());
    }

    [Fact]
    public void GetResponseDirectoryWithSingleFolderShouldAppendDefaultDir()
    {
        var folders = new[] { "only" };

        var result = folders.GetResponseDirectory();

        Assert.Equal(Path.Join("only", DomainDefaults.ResponseDir), result);
    }

    [Fact]
    public void ShowPromptShouldWrapInFence()
    {
        var result = "Hello World".ShowPrompt();

        Assert.Contains("Hello World", result, StringComparison.InvariantCultureIgnoreCase);
        Assert.Contains(PromptFormatting.Fence, result, StringComparison.CurrentCulture);
        Assert.Contains(PromptFormatting.OriginalPromptLabel, result, StringComparison.CurrentCulture);
    }

    [Fact]
    public void ShowPromptWithEmptyStringShouldStillWrap()
    {
        var result = "".ShowPrompt();

        Assert.Contains(PromptFormatting.Fence, result, StringComparison.CurrentCulture);
        Assert.Contains(PromptFormatting.OriginalPromptLabel, result, StringComparison.CurrentCulture);
    }

    [Fact]
    public void ShowPromptShouldStartWithNewlines()
    {
        var result = "Test".ShowPrompt();

        Assert.StartsWith("\n\n", result, StringComparison.CurrentCulture);
    }
}
