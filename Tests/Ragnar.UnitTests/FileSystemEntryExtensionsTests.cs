//using System.IO.Enumeration;

//namespace Ragnar.Tests;

//public class FileSystemEntryExtensionsTests
//{
//    //[Fact]
//    //public void HasAllowedExtensionWithMatchingExtensionShouldReturnTrue()
//    //{
//    //    // Arrange
//    //    var entry = new FileSystemEntry(new FileInfo("test.cs"));
//    //    var allowed = new[] { ".cs", ".json" };

//    //    // Act
//    //    var result = entry.HasAllowedExtension(allowed);

//    //    // Assert
//    //    Assert.True(result);
//    //}

//    //[Fact]
//    //public void HasAllowedExtensionWithNonMatchingExtensionShouldReturnFalse()
//    //{
//    //    // Arrange
//    //    var entry = new FileSystemEntry(new FileInfo("test.pdf"));
//    //    var allowed = new[] { ".cs", ".json" };

//    //    // Act
//    //    var result = entry.HasAllowedExtension(allowed);

//    //    // Assert
//    //    Assert.False(result);
//    //}

//    [Fact]
//    public void HasAllowedExtensionWithDirectoryShouldReturnFalse()
//    {
//        // Arrange
//        var entry = new FileSystemEntry(new DirectoryInfo("someFolder"));
//        var allowed = new[] { ".cs" };

//        // Act
//        var result = entry.HasAllowedExtension(allowed);

//        // Assert
//        Assert.False(result);
//    }

//    [Fact]
//    public void HasAllowedExtensionWithCaseInsensitiveMatchShouldReturnTrue()
//    {
//        // Arrange
//        var entry = new FileSystemEntry(new FileInfo("TEST.CS"));
//        var allowed = new[] { ".cs" };

//        // Act
//        var result = entry.HasAllowedExtension(allowed);

//        // Assert
//        Assert.True(result);
//    }

//    [Fact]
//    public void HasAllowedExtensionWithEmptyAllowedListShouldReturnFalse()
//    {
//        // Arrange
//        var entry = new FileSystemEntry(new FileInfo("test.cs"));
//        var allowed = new string[] { };

//        // Act
//        var result = entry.HasAllowedExtension(allowed);

//        // Assert
//        Assert.False(result);
//    }

//    [Fact]
//    public void HasAllowedExtensionWithNullAllowedListShouldThrow()
//    {
//        // Arrange
//        var entry = new FileSystemEntry(new FileInfo("test.cs"));

//        // Act & Assert
//        Assert.ThrowsAny<ArgumentNullException>(() => entry.HasAllowedExtension(null!));
//    }
//}
