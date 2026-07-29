namespace Ragnar.IntegrationTests;

public class LoadCustomFilesTests : IDisposable
{
    private readonly string _tempDir;

    public LoadCustomFilesTests ()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    [Fact]
    public async Task GetFilesAsync_Should_FilterByExtension ()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "valid.cs"), "// code");
        await File.WriteAllTextAsync(Path.Combine(_tempDir, "ignore.txt"), "not code");
        var filter = new FileLoadOptions { AllowedFileExtensions = [".cs"] };
        var validator = new FileValidator();

        // Act
        var files = await LoadCustomFiles.GetFilesAsync(_tempDir, filter, new EnumerationOptions(), validator, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Single(files);
        Assert.Contains("valid.cs", files[0]);
    }

    //[Fact]
    //public async Task GetFilesAsyncExcludesExcludedFiles ()
    //{
    //    // Arrange
    //    File.WriteAllText(Path.Combine(_tempDir, "Program.cs"), "// code");
    //    File.WriteAllText(Path.Combine(_tempDir, "Test.cs"), "// test");
    //    var filter = new FileLoadOptions { ExcludedFiles = ["Test.cs"] };
    //    var validator = new FileValidator();

    //    // Act
    //    var files = await LoadCustomFiles.GetFilesAsync(_tempDir, filter, new EnumerationOptions(), validator, CancellationToken.None).ToListAsync();

    // Assert
    //Assert.Single(files);
    //    Assert.Contains("Program.cs", files[0]);
    //}

    public void Dispose ()
    {
        Directory.Delete(_tempDir, recursive: true);
        GC.SuppressFinalize(this);
    }
}
