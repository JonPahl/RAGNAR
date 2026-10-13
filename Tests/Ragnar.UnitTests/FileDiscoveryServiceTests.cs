namespace Ragnar.Tests;

public class FileDiscoveryServiceTests : IDisposable
{
    private readonly FileDiscoveryService _sut;
    private readonly string _tempRoot;

    public FileDiscoveryServiceTests()
    {
        FileLoadOptions options = new();
        IFileEligibilityValidator validator = new FileValidator(options);
        _sut = new FileDiscoveryService(validator);
        _tempRoot = Path.Combine(Path.GetTempPath(), $"filetest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    //[Fact]
    //public async Task GetFilesAsyncWithNullFilterShouldThrowArgumentNullException()
    //{
    //    // Act & Assert
    //    await Assert.ThrowsAnyAsync<ArgumentNullException>(
    //        () => _sut.GetFilesAsync(_tempRoot, null!, cancellationToken: CancellationToken.None)
    //        .ToListAsync());
    //}

    //[Fact]
    //public async Task GetFilesAsyncWithEmptyDirectoryPathShouldThrow()
    //{
    //    // Act & Assert
    //    await Assert.ThrowsAnyAsync<ArgumentNullException>(
    //        () => _sut.GetFilesAsync("", new FileLoadOptions(), CancellationToken.None).ToListAsync());
    //}

    [Fact]
    public async Task GetFilesAsyncWithNonExistentDirectoryShouldThrowDirectoryNotFoundException()
    {
        // Arrange
        var opts = new FileLoadOptions { AllowedExtensions = [".cs"], ExcludedDirectories = [] };

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            async () => await _sut.GetFilesAsync("/nonexistent/path/xyz", opts, CancellationToken.None).ToListAsync(cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false));
    }

    [Fact]
    public async Task GetFilesAsyncShouldReturnMatchingFiles()
    {
        // Arrange
        CreateFile("Program.cs", "class A {}");
        CreateFile("Helper.cs", "class B {}");
        CreateFile("README.md", "hello");

        var opts = new FileLoadOptions { AllowedExtensions = [".cs"], ExcludedDirectories = [] };

        // Act
        var results = new List<string>();
        await foreach (var f in _sut.GetFilesAsync(_tempRoot, opts, CancellationToken.None))
            results.Add(f);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, f => Assert.EndsWith(".cs", f, StringComparison.InvariantCultureIgnoreCase));
    }

    [Fact]
    public async Task GetFilesAsyncShouldExcludeSpecifiedDirectories()
    {
        // Arrange
        Directory.CreateDirectory(Path.Combine(_tempRoot, "ExcludedDir"));
        CreateFile("Good.cs", "class A {}");
        CreateFile(Path.Combine("ExcludedDir", "Bad.cs"), "class B {}");

        var opts = new FileLoadOptions
        {
            AllowedExtensions = [".cs"],
            ExcludedDirectories = ["ExcludedDir"]
        };

        // Act
        var results = new List<string>();
        await foreach (var f in _sut.GetFilesAsync(_tempRoot, opts, CancellationToken.None).ConfigureAwait(false))
            results.Add(f);

        // Assert
        Assert.Single(results);
        Assert.EndsWith("Good.cs", results[0], StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public async Task GetFilesAsyncShouldRespectCancellation()
    {
        // Arrange
        CreateFile("A.cs", "x");
        CreateFile("B.cs", "x");
        var opts = new FileLoadOptions { AllowedExtensions = [".cs"], ExcludedDirectories = [] };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await _sut.GetFilesAsync(_tempRoot, opts, cts.Token).ToListAsync(cancellationToken: TestContext.Current.CancellationToken).ConfigureAwait(false));
    }

    [Fact]
    public async Task GetFilesAsyncWithNoMatchingExtensionsShouldReturnEmpty()
    {
        // Arrange
        CreateFile("Doc.pdf", "data");
        var opts = new FileLoadOptions { AllowedExtensions = [".cs"], ExcludedDirectories = [] };

        // Act
        var results = new List<string>();
        await foreach (var f in _sut.GetFilesAsync(_tempRoot, opts, CancellationToken.None).ConfigureAwait(false))
            results.Add(f);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetFilesAsyncWithCaseInsensitiveExtensionShouldMatch()
    {
        // Arrange
        CreateFile("TEST.CS", "class A {}");
        var opts = new FileLoadOptions { AllowedExtensions = [".cs"], ExcludedDirectories = [] };

        // Act
        var results = new List<string>();
        await foreach (var f in _sut.GetFilesAsync(_tempRoot, opts, CancellationToken.None).ConfigureAwait(false))
            results.Add(f);

        // Assert
        Assert.Single(results);
    }

    private void CreateFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(_tempRoot, relativePath);
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(fullPath, content);
    }
}
