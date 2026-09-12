//namespace Ragnar.Tests;

//// ───────────────────────────────────────────────────────────────
////  10. FileSystemEntryExtensions
//// ───────────────────────────────────────────────────────────────
//public class FileSystemEntryExtensionsTests
//{
//    [Fact]
//    public void HasAllowedExtensionReturnsTrueWhenExtensionMatches()
//    {
//        var entry = new FileSystemEntry(new FileInfo("test.cs"));
//        Assert.True(entry.HasAllowedExtension(new[] { ".cs", ".json" }));
//    }

//    [Fact]
//    public void HasAllowedExtensionReturnsTrueCaseInsensitive()
//    {
//        var entry = new FileSystemEntry(new FileInfo("test.CS"));
//        Assert.True(entry.HasAllowedExtension(new[] { ".cs" }));
//    }

//    [Fact]
//    public void HasAllowedExtensionReturnsFalseWhenExtensionNotInList()
//    {
//        var entry = new FileSystemEntry(new FileInfo("test.pdf"));
//        Assert.False(entry.HasAllowedExtension(new[] { ".cs", ".json" }));
//    }

//    [Fact]
//    public void HasAllowedExtensionReturnsFalseForDirectory()
//    {
//        var dir = Path.Combine(Path.GetTempPath(), "RagnarTestDir_" + Guid.NewGuid().ToString("N")[[.. 6]]);
//        Directory.CreateDirectory(dir);
//        try
//        {
//            var entry = new FileSystemEntry(new DirectoryInfo(dir));
//            Assert.False(entry.HasAllowedExtension(new[] { ".cs" }));
//        }
//        finally
//        {
//            Directory.Delete(dir, true);
//        }
//    }

//    [Fact]
//    public void HasAllowedExtensionThrowsWhenCollectionIsNull()
//    {
//        var entry = new FileSystemEntry(new FileInfo("test.cs"));
//        Assert.Throws<ArgumentNullException>(() => entry.HasAllowedExtension(null!));
//    }

//    [Fact]
//    public void HasAllowedExtensionReturnsFalseWhenExtensionIsEmpty()
//    {
//        FileSystemEntry entry = new FileSystemEntry(new FileInfo("noext"));
//        Assert.False(entry.HasAllowedExtension(new[] { ".cs" }));
//    }
//}
