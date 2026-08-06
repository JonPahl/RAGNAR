namespace Ragnar.IntegrationTests;

public class FileSystemEntryExtensionsTests
{
    [Theory]
    [InlineData("file.pdf", new[] { ".pdf", ".docx" }, true)]
    [InlineData("file.txt", new[] { ".pdf", ".docx" }, false)]
    [InlineData("folder", new[] { ".pdf" }, false)] // directory
    public void HasAllowedExtension_Should_Check_Extension(string FileName, string[] Allowed, bool Expected)
    {
        var entry = new FileSystemEntry(new FileInfo(FileName));
        var result = entry.HasAllowedExtension(Allowed);
        Assert.Equal(Expected, result);
    }
}
