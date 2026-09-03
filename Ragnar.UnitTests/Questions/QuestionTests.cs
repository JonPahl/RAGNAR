using Ragnar.Core.Utils;
using Ragnar.Extensions;
using Ragnar.Plugins;

namespace RAGNAR.UnitTests.Questions;

public sealed class QuestionTests
{
    [Fact]
    public void IsActive_CreatesEnabledQuestion()
    {
        // Act
        var question = Ragnar.Core.Model.Question.IsActive("Test?", "test", QuestionCategory.Refactor);

        // Assert
        Assert.True(question.IsEnabled);
        Assert.Equal("Test?", question.Text);
        Assert.Equal("test", question.Filename);
        Assert.Equal(QuestionCategory.Refactor, question.Category);
    }

    [Fact]
    public void IsDisabled_CreatesDisabledQuestion()
    {
        // Act
        var question = Ragnar.Core.Model.Question.IsDisabled("Disabled?", "disabled", QuestionCategory.Logging);

        // Assert
        Assert.False(question.IsEnabled);
        Assert.Equal("Disabled?", question.Text);
        Assert.Equal("disabled", question.Filename);
        Assert.Equal(QuestionCategory.Logging, question.Category);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsActive_Throws_WhenTextIsInvalid(string? text)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Ragnar.Core.Model.Question.IsActive(text!, "key", QuestionCategory.Refactor));
    }


    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsDisabled_Throws_WhenKeyIsInvalid(string? key)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Ragnar.Core.Model.Question.IsDisabled("Text", key!, QuestionCategory.Refactor));
    }


    [Fact]
    public void FromFilePathAndIndex_GeneratesDeterministicId()
    {
        // Arrange
        const string path = "test.cs";
        const string index = "5";

        // Act
        var id1 = Point.FromFilePathAndIndex(path.AsSpan(), index.AsSpan());
        var id2 = Point.FromFilePathAndIndex(path.AsSpan(), index.AsSpan());

        // Assert
        Assert.Equal(id1, id2);
        Assert.NotEqual(0UL, id1);
    }

    [Fact]
    public void FromFilePathAndIndex_HandlesLongPaths()
    {
        // Arrange
        const string path = "very/long/path/to/a/file/with/many/directories/Program.cs";
        const string index = "42";

        // Act
        var id = Point.FromFilePathAndIndex(path.AsSpan(), index.AsSpan());

        // Assert
        Assert.NotEqual(0UL, id);
    }

    [Fact]
    public void GetResponseDirectory_AppendsFolders()
    {
        // Arrange
        var folders = new List<string> { "Category1", "Category2" };
        const string baseDir = "/output";

        // Act
        var result = folders.GetResponseDirectory(baseDir);

        // Assert
        Assert.Equal(Path.Combine("/output", "Category1", "Category2"), result);
    }

    [Fact]
    public void GetResponseDirectory_WithEmptyFoldersAndNoBaseDir()
    {
        // Arrange
        var folders = Array.Empty<string>();

        // Act
        var result = folders.GetResponseDirectory();

        // Assert
        Assert.Equal("Response", result);
    }

    //[Fact]
    //public void IsExcluded_ReturnsTrue_ForMatchingExclusion()
    //{
    //    // Arrange
    //    var fileName = "Program.cs";
    //    var exclusions = new List<string> = ["Program.cs", "bin"];

    //    // Act
    //    var result = fileName.AsSpan().IsExcluded(in exclusions);

    //    // Assert
    //    Assert.True(result);
    //}

    //[Fact]
    //public void IsExcluded_IgnoresCase()
    //{
    //    // Arrange
    //    var fileName = "program.cs";
    //    var exclusions = ["PROGRAM.CS"];

    //    // Act
    //    var result = fileName.AsSpan().IsExcluded(in exclusions);

    //    // Assert
    //    Assert.True(result);
    //}
}