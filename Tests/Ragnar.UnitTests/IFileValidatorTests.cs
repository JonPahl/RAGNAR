namespace Ragnar.Tests;

public class IFileValidatorTests
{
    [Fact]
    public void IsValidWithNullFileShouldThrowArgumentNullException()
    {
        var validator = new Mock<IFileValidator>();
        var opts = new FileLoadOptions { AllowedExtensions = [".cs"], ExcludedDirectories = [] };

        Assert.ThrowsAny<ArgumentNullException>(() =>
            validator.Object.IsValid(null!, in opts));
    }

    [Fact]
    public void IsValidShouldReturnTrueForAllowedFile()
    {
        var validator = new Mock<IFileValidator>();
        validator.Setup(v => v.IsValid(It.IsAny<FileInfo>(), It.Is<FileLoadOptions>(o => o.AllowedExtensions.Contains(".cs"))))
                 .Returns(true);

        var fi = new FileInfo("test.cs");
        var opts = new FileLoadOptions { AllowedExtensions = [".cs"], ExcludedDirectories = [] };

        Assert.True(validator.Object.IsValid(fi, in opts));
    }

    [Fact]
    public void IsValidShouldReturnFalseForDisallowedFile()
    {
        var validator = new Mock<IFileValidator>();
        validator.Setup(v => v.IsValid(It.IsAny<FileInfo>(), It.Is<FileLoadOptions>(o => !o.AllowedExtensions.Contains(".exe"))))
                 .Returns(false);

        var fi = new FileInfo("malware.exe");
        var opts = new FileLoadOptions { AllowedExtensions = [".cs"], ExcludedDirectories = [] };

        Assert.False(validator.Object.IsValid(fi, in opts));
    }
}
