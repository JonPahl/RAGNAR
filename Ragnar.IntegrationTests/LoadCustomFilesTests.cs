namespace Ragnar.IntegrationTests;

public class LoadCustomFilesTests : IDisposable
{
    private readonly string TempDir;

    public LoadCustomFilesTests()
    {
        TempDir = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}");
        Directory.CreateDirectory(TempDir);
    }

    [Fact]
    public async Task GetFilesAsyncShouldFilterByExtension()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(TempDir, "valid.cs"), "// code");
        await File.WriteAllTextAsync(Path.Combine(TempDir, "ignore.txt"), "not code");
        var Filter = new FileLoadOptions { AllowedFileExtensions = [".cs"] };
        var Validator = new FileValidator();

        // Act
        var Files = await LoadCustomFiles.GetFilesAsync(TempDir, Filter, new EnumerationOptions(), Validator, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Single(Files);
        Assert.Contains("valid.cs", Files[0]);
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

    public void Dispose()
    {
        Directory.Delete(TempDir, recursive: true);
        GC.SuppressFinalize(this);
    }
}
