namespace Ragnar.Tests;

public class FileDiscoveryServiceTests : IDisposable
{
    private readonly Mock<IFileValidator> _validatorMock;
    private readonly FileDiscoveryService _sut;
    private string _testDir;

    public FileDiscoveryServiceTests()
    {
        _validatorMock = new Mock<IFileValidator>();
        _sut = new FileDiscoveryService();
        _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    private string CreateTestFile(string relativePath, string content = "test")
    {
        var fullPath = Path.Combine(_testDir, relativePath);
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(fullPath, content);
        return fullPath;
    }

    [Fact]
    public void GetFilesAsyncWithNullFilterShouldThrowArgumentNullException()
    {
        Directory.CreateDirectory(_testDir);

        var ex = Assert.Throws<ArgumentNullException>(() =>
        {
            var _ = _sut.GetFilesAsync(_testDir, null!, CancellationToken.None);
        });
    }

    [Fact]
    public void GetFilesAsyncWithEmptyDirectoryShouldThrowArgumentException()
    {
        Directory.CreateDirectory(_testDir);

        Assert.ThrowsAny<ArgumentException>(() =>
        {
            var opts = new FileLoadOptions { AllowedExtensions = [], ExcludedDirectories = [] };
            var _ = _sut.GetFilesAsync(_testDir, opts, CancellationToken.None);
        });
    }

    [Fact]
    public void GetFilesAsyncWithNonExistentDirectoryShouldThrowDirectoryNotFound()
    {
        var nonExistent = Path.Combine(_testDir, "does_not_exist");
        var opts = new FileLoadOptions { AllowedExtensions = [".cs"], ExcludedDirectories = [] };

        Assert.Throws<DirectoryNotFoundException>(
            () => { var _ = _sut.GetFilesAsync(nonExistent, opts, CancellationToken.None); });
    }

    [Fact]
    public async Task GetFilesAsyncShouldReturnMatchingFiles()
    {
        CreateTestFile("source/Program.cs", "class P {}");
        CreateTestFile("source/Helper.cs", "class H {}");
        CreateTestFile("source/readme.md", "# Hello");

        var opts = new FileLoadOptions
        {
            AllowedExtensions = [".cs"],
            ExcludedDirectories = []
        };

        var results = new List<string>();
        await foreach (var file in _sut.GetFilesAsync(_testDir, opts, CancellationToken.None))
        {
            results.Add(file);
        }

        Assert.Equal(2, results.Count);
        Assert.All(results, f => Assert.EndsWith(".cs", f, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetFilesAsyncShouldExcludeSpecifiedDirectories()
    {
        CreateTestFile("src/Main.cs", "class M {}");
        CreateTestFile("obj/Generated.cs", "class G {}");
        CreateTestFile("bin/Output.cs", "class O {}");

        var opts = new FileLoadOptions
        {
            AllowedExtensions = [".cs"],
            ExcludedDirectories = ["obj", "bin"]
        };

        var results = new List<string>();
        await foreach (var file in _sut.GetFilesAsync(_testDir, opts, CancellationToken.None))
        {
            results.Add(file);
        }

        Assert.Single(results);
        Assert.Contains("src", results[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetFilesAsyncShouldFilterByAllowedExtensions()
    {
        CreateTestFile("a.cs");
        CreateTestFile("b.json");
        CreateTestFile("c.xml");

        var opts = new FileLoadOptions
        {
            AllowedExtensions = [".json", ".xml"],
            ExcludedDirectories = []
        };

        var results = new List<string>();
        await foreach (var file in _sut.GetFilesAsync(_testDir, opts, CancellationToken.None))
        {
            results.Add(file);
        }

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetFilesAsyncWithCancellationShouldThrow()
    {
        CreateTestFile("a.cs");
        var opts = new FileLoadOptions
        {
            AllowedExtensions = [".cs"],
            ExcludedDirectories = []
        };

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () =>
            {
                await foreach (var _ in _sut.GetFilesAsync(_testDir, opts, cts.Token).ConfigureAwait(false))
                { }
            });
    }

    [Fact]
    public async Task GetFilesAsyncShouldReturnEmptyWhenNoMatches()
    {
        CreateTestFile("a.txt");

        var opts = new FileLoadOptions
        {
            AllowedExtensions = [".cs"],
            ExcludedDirectories = []
        };

        var results = new List<string>();
        await foreach (var file in _sut.GetFilesAsync(_testDir, opts, CancellationToken.None))
        {
            results.Add(file);
        }

        Assert.Empty(results);
    }
}
