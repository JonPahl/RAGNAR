using Ragnar.Core.Options;
using Ragnar.Utils;

namespace RAGNAR.UnitTests.Embedding;

public sealed class FileValidatorTests
{
    [Fact]
    public void IsValid_ReturnsTrue_ForValidFile()
    {
        var validator = new FileValidator();
        var file = new FileInfo("Program.cs");
        var filter = new FileLoadOptions
        {
            AllowedFileExtensions = [".cs"],
            ExcludedFiles = [],
            ExcludedDirectories = []
        };

        Assert.True(validator.IsValid(file, in filter));
    }
}
