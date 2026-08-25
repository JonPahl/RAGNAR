namespace Ragnar.Tests;

public class ExtensionMethodsTests
{
    [Fact]
    public void GetResponseDirectory_String_ReturnsCombinedPath()
    {
        const string BASEDIR = "/base/path";
        var result = BASEDIR.GetResponseDirectory();
        result.Should().Be(Path.Combine("/base/path", "Response"));
    }

    [Fact]
    public void ShowPrompt_WrapsTextInMarkdownFences()
    {
        const string PROMPT = "Explain dependency injection";
        var result = PROMPT.ShowPrompt();
        result.Should().Contain("***");
        result.Should().Contain("[Original Prompt]");
        result.Should().Contain(PROMPT);
    }

    [Fact]
    public void ExpandDirectory_ReturnsFullPath_WhenDirExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = tempDir.ExpandDirectory();
            Directory.Exists(result).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ExpandDirectory_ThrowsDirectoryNotFoundException_WhenMissing()
    {
        var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Assert.Throws<DirectoryNotFoundException>(() => nonExistent.ExpandDirectory());
    }

    [Fact]
    public void ElapsedTimeString_FormatsAsMmSs()
    {
        var sw = Stopwatch.StartNew();
        Task.Delay(50, TestContext.Current.CancellationToken).Wait(TestContext.Current.CancellationToken);

        var formatted = sw.ElapsedTimeString();
        formatted.Should().MatchRegex(@"^\d{2}:\d{2}$");
    }

    [Fact]
    public void ExpandDirectory_ReturnsFullPath_WhenExists()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);

        var result = dir.ExpandDirectory();
        result.Should().Be(System.IO.Path.GetFullPath(dir));

        Directory.Delete(dir, true);
    }

    [Fact]
    public void ExpandDirectory_ThrowsWhenMissing()
    {
        var fakePath = @"C:\NonExistentDir_" + Guid.NewGuid();
        Assert.Throws<DirectoryNotFoundException>(() => fakePath.ExpandDirectory());
    }

    [Fact]
    public void InformationalVersion_ReturnsFallback_WhenAttributeMissing()
    {
        var asmMock = new Mock<IAssemblyInfo>();
        asmMock.Setup(a => a.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>())
               .Returns((AssemblyInformationalVersionAttribute)null);

        var version = asmMock.Object.InformationalVersion;
        version.Should().Be("1.0.0");
    }

    [Fact]
    public void GetStyle_ReturnsPlain_WhenNull()
    {
        Style? nullStyle = null;
        var result = nullStyle.GetStyle;
        result.Should().Be(Style.Plain);
    }
}
