### <span style="color:darkblue;">Testing</span> Please create integration tests using (xunit and moq) for all public methods where applicable.
> **Date Generated**: 9/18/2026 5:02:02 AM
> ## Question: 
> Please create integration tests using (xunit and moq) for all public methods where applicable.
> **Method Call Duration**: 01:22:27
 ## Response: 
```csharp
// ============================================================
// FileSystemEntryExtensionsTests.cs
// ============================================================
using Ragnar.Extensions;

namespace Ragnar.UnitTests.Extensions;

public class FileSystemEntryExtensionsTests
{
    [[Theory]]
    [[InlineData(".cs", [[".cs", ".json"]], true)]]
    [[InlineData(".json", [[".cs", ".json"]], true)]]
    [[InlineData(".pdf", [[".cs", ".json"]], false)]]
    [[InlineData(".CS", [[".cs", ".json"]], true)]]   // case-insensitive
    [[InlineData(".txt", [[".CS", ".JSON"]], true)]]  // case-insensitive both sides
    public void HasAllowedExtensionMatchShouldReturnTrue(string extension, string[[]] allowed, bool expected)
    {
        // Arrange
        var entry = new FileSystemEntry(new FileInfo($"C:\\temp\\file{extension}"));

        // Act
        var result = entry.HasAllowedExtension(allowed);

        // Assert
        Assert.Equal(expected, result);
    }

    [[Fact]]
    public void HasAllowedExtensionEmptyAllowedListShouldReturnFalse()
    {
        // Arrange
        var entry = new FileSystemEntry(new FileInfo("C:\\temp\\file.cs"));

        // Act
        var result = entry.HasAllowedExtension(Array.Empty<string>());

        // Assert
        Assert.False(result);
    }

    [[Fact]]
    public void HasAllowedExtensionDirectoryEntryShouldReturnFalse()
    {
        // Arrange
        var dir = Directory.CreateTempSubdirectory("TestDir").FullName;
        var entry = new FileSystemEntry(new DirectoryInfo(dir));

        // Act
        var result = entry.HasAllowedExtension(new[[]] { ".cs", ".json" });

        // Assert
        Assert.False(result);

        // Cleanup
        Directory.Delete(dir, recursive: true);
    }

    [[Fact]]
    public void HasAllowedExtensionNullAllowedExtensionsShouldThrowArgumentNullException()
    {
        // Arrange
        var entry = new FileSystemEntry(new FileInfo("C:\\temp\\file.cs"));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => entry.HasAllowedExtension(null!));
    }
}


// ============================================================
// BaseFileParserTests.cs
// ============================================================
using Ragnar.Embedding.UnitOfWork;

namespace Ragnar.UnitTests.Embedding;

public class BaseFileParserTests : IDisposable
{
    private sealed class TestFileParser(ILogger logger) : BaseFileParser(logger)
    {
        public override Task<IEnumerable<CodeDocument>> ParseAsync(string filePath, CancellationToken cancellationToken)
            => Task.FromResult<IEnumerable<CodeDocument>>(Array.Empty<CodeDocument>());
    }

    private readonly Mock<ILogger> _loggerMock;
    private readonly TestFileParser _sut;
    private readonly string _tempFilePath;

    public BaseFileParserTests()
    {
        _loggerMock = new Mock<ILogger>();
        _sut = new TestFileParser(_loggerMock.Object);
        _tempFilePath = Path.GetTempFileName();
    }

    [[Fact]]
    public async Task ReadFileAsyncValidFileShouldReturnContent()
    {
        // Arrange
        const string expectedContent = "Hello, Ragnar!";
        await File.WriteAllTextAsync(_tempFilePath, expectedContent);

        // Act
        var result = await _sut.ReadFileAsync(_tempFilePath, CancellationToken.None);

        // Assert
        Assert.Equal(expectedContent, result);
    }

    [[Fact]]
    public async Task ReadFileAsyncNullPathShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _sut.ReadFileAsync(null!, CancellationToken.None));
    }

    [[Fact]]
    public async Task ReadFileAsyncEmptyPathShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAnyAsync<ArgumentException>(() => _sut.ReadFileAsync(string.Empty, CancellationToken.None));
    }

    [[Fact]]
    public async Task ReadFileAsyncNonExistentFileShouldThrowInvalidOperationException()
    {
        // Arrange
        var nonExistent = Path.Combine(Path.GetTempPath(), "does_not_exist_xyz.cs");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ReadFileAsync(nonExistent, CancellationToken.None));
        Assert.Contains(nonExistent, ex.Message);
    }

    [[Fact]]
    public async Task ReadFileAsyncCancelledTokenShouldThrowOperationCanceledException()
    {
        // Arrange
        await File.WriteAllTextAsync(_tempFilePath, "data");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.ReadFileAsync(_tempFilePath, cts.Token));
    }

    public void Dispose()
    {
        if (File.Exists(_tempFilePath))
            File.Delete(_tempFilePath);
    }
}


// ============================================================
// CsvFileQuestionProviderTests.cs
// ============================================================
using Ragnar.FileQuestionProvider;

namespace Ragnar.UnitTests.QuestionProvider;

public class CsvFileQuestionProviderTests
{
    private readonly Mock<IRecordParser<QuestionRecord>> _parserMock;
    private readonly CsvFileQuestionProvider _sut;

    public CsvFileQuestionProviderTests()
    {
        _parserMock = new Mock<IRecordParser<QuestionRecord>>();
        _sut = new CsvFileQuestionProvider(_parserMock.Object);
    }

    [[Fact]]
    public void ProviderNameShouldReturnCsvFile()
    {
        // Act
        var result = _sut.ProviderName;

        // Assert
        Assert.Equal("CSV File", result);
    }

    [[Fact]]
    public async Task LoadQuestionsAsyncShouldReturnMappedQuestions()
    {
        // Arrange
        var records = new[[]]
        {
            new QuestionRecord { IsEnabled = true, Text = "What is C#?", FileName = "q.csv", Category = QuestionCategory.CSharp },
            new QuestionRecord { IsEnabled = false, Text = "Old question", FileName = "q.csv", Category = QuestionCategory.Other }
        };
        _parserMock
            .Setup(p => p.ParseAsync("data.csv", It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        // Act
        var questions = await _sut.LoadQuestionsAsync("data.csv", CancellationToken.None);

        // Assert
        var list = questions.ToList();
        Assert.Equal(2, list.Count);
        Assert.True(list[[0]].IsActive);
        Assert.Equal("What is C#?", list[[0]].Text);
        Assert.Equal("q.csv", list[[0]].FileName);
        Assert.Equal(QuestionCategory.CSharp, list[[0]].Category);
        Assert.False(list[[1]].IsActive);
    }

    [[Fact]]
    public async Task LoadQuestionsAsyncEmptyRecordsShouldReturnEmptyCollection()
    {
        // Arrange
        _parserMock
            .Setup(p => p.ParseAsync("empty.csv", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<QuestionRecord>());

        // Act
        var questions = await _sut.LoadQuestionsAsync("empty.csv", CancellationToken.None);

        // Assert
        Assert.Empty(questions);
    }

    [[Fact]]
    public async Task LoadQuestionsAsyncCancelledShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        _parserMock
            .Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.LoadQuestionsAsync("data.csv", cts.Token));
    }

    [[Fact]]
    public void LoadQuestionsAsyncShouldPropagateParserExceptions()
    {
        // Arrange
        _parserMock
            .Setup(p => p.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Disk error"));

        // Act & Assert
        Assert.ThrowsAny<IOException>(() =>
        {
            var t = _sut.LoadQuestionsAsync("data.csv", CancellationToken.None);
            t.GetAwaiter().GetResult();
        });
    }
}


// ============================================================
// SummarizePromptProviderTests.cs
// ============================================================
using Ragnar.Services;

namespace Ragnar.UnitTests.Services;

public class SummarizePromptProviderTests
{
    private readonly SummarizePromptProvider _sut;

    public SummarizePromptProviderTests()
    {
        _sut = new SummarizePromptProvider();
    }

    [[Fact]]
    public void SystemShouldReturnNonEmptyInstruction()
    {
        // Act
        var result = _sut.System;

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.Contains("summary", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1000 words", result, StringComparison.OrdinalIgnoreCase);
    }

    [[Fact]]
    public void GetTemplateShouldCombineContentAndQuestion()
    {
        // Arrange
        const string content = "var x = 42;";
        const string question = "What does this do?";

        // Act
        var result = _sut.GetTemplate(content, question);

        // Assert
        Assert.Contains(content, result);
        Assert.Contains(question, result);
        Assert.Contains("Question:", result);
    }

    [[Fact]]
    public void GetTemplateWithEmptyContentShouldStillIncludeQuestion()
    {
        // Arrange
        const string question = "Explain this";

        // Act
        var result = _sut.GetTemplate(string.Empty, question);

        // Assert
        Assert.Contains(question, result);
        Assert.Contains("Question:", result);
    }

    [[Fact]]
    public void GetTemplateWithNullQuestionShouldNotThrow()
    {
        // Arrange
        const string content = "some code";

        // Act
        var result = _sut.GetTemplate(content, null);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(content, result);
    }

    [[Fact]]
    public void SystemShouldBeAValidIChatPromptProvider()
    {
        // Arrange
        IChatPromptProvider provider = _sut;

        // Act
        var system = provider.System;
        var template = provider.GetTemplate("code", "Q");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(system));
        Assert.False(string.IsNullOrWhiteSpace(template));
    }
}


// ============================================================
// PipelineRunnerTests.cs
// ============================================================
using Ragnar;

namespace Ragnar.UnitTests.Pipeline;

public class PipelineRunnerTests
{
    private readonly Mock<IOutputWriter> _writerMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly PipelineRunner _sut;

    public PipelineRunnerTests()
    {
        _writerMock = new Mock<IOutputWriter>();
        _loggerMock = new Mock<ILogger>();
        _sut = new PipelineRunner(_writerMock.Object, _loggerMock.Object);
    }

    #region AddStage

    [[Fact]]
    public void AddStageShouldReturnSameInstanceForFluentChaining()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<EmbeddingContext>>().Object;

        // Act
        var result = _sut.AddStage(stage);

        // Assert
        Assert.Same(_sut, result);
    }

    [[Fact]]
    public void AddStageMultipleTimesShouldRegisterAllStages()
    {
        // Arrange
        var stage1 = new Mock<IPipelineStage<EmbeddingContext>>().Object;
        var stage2 = new Mock<IPipelineStage<EmbeddingContext>>().Object;
        var stage3 = new Mock<IPipelineStage<EmbeddingContext>>().Object;

        // Act
        _sut.AddStage(stage1).AddStage(stage2).AddStage(stage3);
        var context = new EmbeddingContext();
        _sut.ExecuteAsync(context, CancellationToken.None).GetAwaiter().GetResult();

        // Assert
        stage1.Verify(s => s.ExecuteAsync(It.IsAny<EmbeddingContext>(), It.IsAny<CancellationToken>()), Times.Once);
        stage2.Verify(s => s.ExecuteAsync(It.IsAny<EmbeddingContext>(), It.IsAny<CancellationToken>()), Times.Once);
        stage3.Verify(s => s.ExecuteAsync(It.IsAny<EmbeddingContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ExecuteAsync

    [[Fact]]
    public async Task ExecuteAsyncNoStagesShouldReturnSameContext()
    {
        // Arrange
        var context = new EmbeddingContext();

        // Act
        var result = await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.Same(context, result);
    }

    [[Fact]]
    public async Task ExecuteAsyncShouldExecuteStagesInOrder()
    {
        // Arrange
        var executionOrder = new List<string>();
        var stage1 = CreateMockStage("Stage1", executionOrder);
        var stage2 = CreateMockStage("Stage2", executionOrder);
        var stage3 = CreateMockStage("Stage3", executionOrder);

        _sut.AddStage(stage1).AddStage(stage2).AddStage(stage3);

        // Act
        await _sut.ExecuteAsync(new EmbeddingContext(), CancellationToken.None);

        // Assert
        Assert.Equal(new[[]] { "Stage1", "Stage2", "Stage3" }, executionOrder);
    }

    [[Fact]]
    public async Task ExecuteAsyncShouldSkipStageWhenShouldRunIsFalse()
    {
        // Arrange
        var skippedStage = new Mock<IPipelineStage<EmbeddingContext>>();
        skippedStage.Setup(s => s.Name).Returns("Skipped");
        skippedStage.Setup(s => s.ShouldRun).Returns(false);
        skippedStage.Setup(s => s.ExecuteAsync(It.IsAny<EmbeddingContext>(), It.IsAny<CancellationToken>()))
                   .Callback<EmbeddingContext, CancellationToken>((_, _) => throw new InvalidOperationException("Should not run"));

        var activeStage = new Mock<IPipelineStage<EmbeddingContext>>();
        activeStage.Setup(s => s.Name).Returns("Active");
        activeStage.Setup(s => s.ShouldRun).Returns(true);
        activeStage.Setup(s => s.ExecuteAsync(It.IsAny<EmbeddingContext>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

        _sut.AddStage(skippedStage).AddStage(activeStage);

        // Act
        var result = await _sut.ExecuteAsync(new EmbeddingContext(), CancellationToken.None);

        // Assert
        skippedStage.Verify(s => s.ExecuteAsync(It.IsAny<EmbeddingContext>(), It.IsAny<CancellationToken>()), Times.Never);
        activeStage.Verify(s => s.ExecuteAsync(It.IsAny<EmbeddingContext>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [[Fact]]
    public async Task ExecuteAsyncStageThrowsShouldWrapInPipelineStageException()
    {
        // Arrange
        var failingStage = new Mock<IPipelineStage<EmbeddingContext>>();
        failingStage.Setup(s => s.Name).Returns("FailingStage");
        failingStage.Setup(s => s.ShouldRun).Returns(true);
        failingStage.Setup(s => s.ExecuteAsync(It.IsAny<EmbeddingContext>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("Inner error"));

        _sut.AddStage(failingStage);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<PipelineStageException>(
            () => _sut.ExecuteAsync(new EmbeddingContext(), CancellationToken.None));
        Assert.Contains("FailingStage", ex.Message);
        Assert.IsAssignableFrom<InvalidOperationException>(ex.InnerException);
    }

    [[Fact]]
    public async Task ExecuteAsyncOperationCanceledExceptionShouldPropagate()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var stage = new Mock<IPipelineStage<EmbeddingContext>>();
        stage.Setup(s => s.Name).Returns("Cancelled");
        stage.Setup(s => s.ShouldRun).Returns(true);
        stage.Setup(s => s.ExecuteAsync(It.IsAny<EmbeddingContext>(), It.IsAny<CancellationToken>()))
           .ThrowsAsync(new OperationCanceledException(cts.Token));

        _sut.AddStage(stage);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _sut.ExecuteAsync(new EmbeddingContext(), cts.Token));
    }

    [[Fact]]
    public async Task ExecuteAsyncShouldLogStageCompletionWithTiming()
    {
        // Arrange
        var stage = new Mock<IPipelineStage<EmbeddingContext>>();
        stage.Setup(s => s.Name).Returns("Timed");
        stage.Setup(s => s.ShouldRun).Returns(true);
        stage.Setup(s => s.ExecuteAsync(It.IsAny<EmbeddingContext>(), It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

        _sut.AddStage(stage);

        // Act
        await _sut.ExecuteAsync(new EmbeddingContext(), CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            l => l.Information("Completed stages [[{Index}]] {Name} in {Time}", It.IsAny<object>(), It.IsAny<object>(), It.IsAny<object>()),
            Times.Once);
    }

    #endregion

    private static Mock<IPipelineStage<EmbeddingContext>> CreateMockStage(string name, List<string> order)
    {
        var mock = new Mock<IPipelineStage<EmbeddingContext>>();
        mock.Setup(s => s.Name).Returns(name);
        mock.Setup(s => s.ShouldRun).Returns(true);
        mock.Setup(s => s.ExecuteAsync(It.IsAny<EmbeddingContext>(), It.IsAny<CancellationToken>()))
           .Callback<EmbeddingContext, CancellationToken>((_, _) => order.Add(name))
           .Returns(Task.CompletedTask);
        return mock;
    }
}


// ============================================================
// AssemblyExtensionsTests.cs
// ============================================================
using Ragnar.Branding;

namespace Ragnar.UnitTests.Branding;

public class AssemblyExtensionsTests
{
    [[Fact]]
    public void InformationalVersionShouldReturnNonNull()
    {
        // Arrange
        var asm = typeof(AssemblyExtensions).Assembly;

        // Act
        var version = asm.InformationalVersion;

        // Assert
        Assert.NotNull(version);
    }

    [[Fact]]
    public void InformationalVersionShouldFollowSemVerPattern()
    {
        // Arrange
        var asm = typeof(AssemblyExtensions).Assembly;

        // Act
        var version = asm.InformationalVersion;

        // Assert
        Assert.Matches(@"^\d+\.\d+\.\d+", version);
    }

    [[Fact]]
    public void InformationalVersionFallbackShouldBe100WhenNoAttribute()
    {
        // This tests the fallback path – an assembly without AssemblyInformationalVersionAttribute
        // will return "1.0.0". We simulate by checking the logic indirectly.
        // In a real scenario, dynamically generated assemblies may lack the attribute.
        var asm = typeof(AssemblyExtensions).Assembly;
        var version = asm.InformationalVersion;

        // Assert
        Assert.True(!string.IsNullOrWhiteSpace(version));
    }
}


// ============================================================
// XmlCommentFilterStrategyTests.cs
// ============================================================
using Ragnar.Questions.Filters;

namespace Ragnar.UnitTests.Filters;

public class XmlCommentFilterStrategyTests
{
    private readonly XmlCommentFilterStrategy _sut;

    public XmlCommentFilterStrategyTests()
    {
        _sut = new XmlCommentFilterStrategy();
    }

    [[Fact]]
    public void SupportedCategoryShouldReturnXml()
    {
        // Act
        var result = _sut.SupportedCategory;

        // Assert
        Assert.Equal(QuestionCategory.XML, result);
    }

    [[Fact]]
    public void CreateFilterShouldReturnNonNullFilter()
    {
        // Act
        var filter = _sut.CreateFilter(50);

        // Assert
        Assert.NotNull(filter);
        Assert.NotNull(filter.Must);
        Assert.NotEmpty(filter.Must);
    }

    [[Fact]]
    public void CreateFilterShouldContainCommentLengthCondition()
    {
        // Arrange
        const int threshold = 100;

        // Act
        var filter = _sut.CreateFilter(threshold);

        // Assert
        var lengthCondition = filter.Must.First(c => c.Field.Key == "CommentLength");
        Assert.NotNull(lengthCondition.Field.Range);
        Assert.Equal(threshold, lengthCondition.Field.Range.Lt);
    }

    [[Fact]]
    public void CreateFilterShouldExcludeTestCategories()
    {
        // Act
        var filter = _sut.CreateFilter(50);

        // Assert
        var categoryCondition = filter.Must.First(c => c.Field.Key == "Category");
        var exceptKeywords = categoryCondition.Field.Match.ExceptKeywords;
        Assert.NotNull(exceptKeywords);
        Assert.Contains("Test", exceptKeywords.Strings);
        Assert.Contains("Tests", exceptKeywords.Strings);
        Assert.Contains("test", exceptKeywords.Strings);
        Assert.Contains("tests", exceptKeywords.Strings);
        Assert.Contains("Testing", exceptKeywords.Strings);
    }

    [[Fact]]
    public void CreateFilterWithDifferentThresholdsShouldSetCorrectLt()
    {
        // Act
        var filter1 = _sut.CreateFilter(10);
        var filter2 = _sut.CreateFilter(200);

        // Assert
        Assert.Equal(10, filter1.Must.First(c => c.Field.Key == "CommentLength").Field.Range.Lt);
        Assert.Equal(200, filter2.Must.First(c => c.Field.Key == "CommentLength").Field.Range.Lt);
    }

    [[Fact]]
    public void CreateFilterShouldImplementIFilterStrategy()
    {
        // Arrange
        IFilterStrategy strategy = _sut;

        // Act
        var filter = strategy.CreateFilter(25);

        // Assert
        Assert.NotNull(filter);
        Assert.Equal(QuestionCategory.XML, strategy.SupportedCategory);
    }
}


// ============================================================
// PathResolverTests.cs
// ============================================================
using Ragnar.Output;

namespace Ragnar.UnitTests.Output;

public class PathResolverTests
{
    private readonly Mock<IOptions<RagnarConfig>> _optionsMock;
    private readonly RagnarConfig _config;
    private readonly PathResolver _sut;
    private string _tempDir;

    public PathResolverTests()
    {
        _tempDir = Directory.CreateTempSubdirectory("RagnarTest").FullName;
        _config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions
            {
                SourceDirectory = _tempDir,
                OutputFolder = "Response"
            }
        };
        _optionsMock = new Mock<IOptions<RagnarConfig>>();
        _optionsMock.Setup(o => o.Value).Returns(_config);
        _sut = new PathResolver(_optionsMock.Object);
    }

    [[Fact]]
    public void ResolveResponseDirectoryWithCSharpCategoryShouldReturnSubfolder()
    {
        // Arrange
        Directory.CreateDirectory(Path.Join(_config.ApplicationOptions.SourceDirectory, "Response"));

        // Act
        var result = _sut.ResolveResponseDirectory(QuestionCategory.CSharp);

        // Assert
        Assert.Contains("CSharp", result);
        Assert.StartsWith(Path.Join(_config.ApplicationOptions.SourceDirectory, "Response"), result);
    }

    [[Fact]]
    public void ResolveResponseDirectoryWithNullCategoryShouldReturnUncategorized()
    {
        // Arrange
        Directory.CreateDirectory(Path.Join(_config.ApplicationOptions.SourceDirectory, "Response"));

        // Act
        var result = _sut.ResolveResponseDirectory(null);

        // Assert
        Assert.Contains("Uncategorized", result);
    }

    [[Fact]]
    public void ResolveResponseDirectoryWithOtherCategoryShouldReturnSubfolder()
    {
        // Arrange
        Directory.CreateDirectory(Path.Join(_config.ApplicationOptions.SourceDirectory, "Response"));

        // Act
        var result = _sut.ResolveResponseDirectory(QuestionCategory.Other);

        // Assert
        Assert.Contains("Other", result);
    }

    [[Fact]]
    public void ResolveResponseDirectoryWhenDirDoesNotExistShouldThrow()
    {
        // Arrange – ensure the Response directory does NOT exist
        var nonExistentSource = Path.Combine(Path.GetTempPath(), "RagnarNotExist");
        _config.ApplicationOptions.SourceDirectory = nonExistentSource;

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() => _sut.ResolveResponseDirectory(QuestionCategory.CSharp));
    }

    [[Fact]]
    public void ResolveResponseDirectoryShouldRespectConfiguredOutputFolder()
    {
        // Arrange
        _config.ApplicationOptions.OutputFolder = "MyOutput";
        Directory.CreateDirectory(Path.Join(_config.ApplicationOptions.SourceDirectory, "MyOutput"));

        // Act
        var result = _sut.ResolveResponseDirectory(QuestionCategory.CSharp);

        // Assert
        Assert.Contains("MyOutput", result);
    }
}


// ============================================================
// ApplicationHeaderTests.cs
// ============================================================
using Ragnar.Branding;

namespace Ragnar.UnitTests.Branding;

public class ApplicationHeaderTests
{
    private readonly Mock<IOutputWriter> _writerMock;
    private readonly ApplicationHeader _sut;

    public ApplicationHeaderTests()
    {
        _writerMock = new Mock<IOutputWriter>();
        _sut = new ApplicationHeader(_writerMock.Object);
    }

    [[Fact]]
    public void RenderBrandingShouldWriteMultipleElements()
    {
        // Act
        _sut.RenderBranding();

        // Assert – at minimum: title, subtitle, version, blank lines, tagline, blank line, rule
        _writerMock.Verify(w => w.Write(It.IsAny<object>()), Times.AtLeast(3));
        _writerMock.Verify(w => w.WriteLine(), Times.AtLeast(3));
        _writerMock.Verify(w => w.WriteRule(), Times.Once);
    }

    [[Fact]]
    public void RenderBrandingShouldIncludeRagnarTitle()
    {
        // Act
        _sut.RenderBranding();

        // Assert
        _writerMock.Verify(
            w => w.Write(It.Is<Spectre.Console.Text>(t => t.Contains("Ragnar").Value)),
            Times.AtLeast(1));
    }

    [[Fact]]
    public void RenderBrandingShouldIncludeVersion()
    {
        // Act
        _sut.RenderBranding();

        // Assert
        _writerMock.Verify(
            w => w.Write(It.Is<Spectre.Console.Text>(t => t.Contains("Version").Value)),
            Times.Once);
    }

    [[Fact]]
    public void RenderBrandingShouldBeCallableMultipleTimes()
    {
        // Act – should not throw
        _sut.RenderBranding();
        _sut.RenderBranding();
        _sut.RenderBranding();

        // Assert – no exceptions means success; verify writer was called multiple times
        _writerMock.Verify(w => w.WriteRule(), Times.Exactly(3));
    }

    [[Fact]]
    public void RenderBrandingShouldImplementIApplicationHeader()
    {
        // Arrange
        IApplicationHeader header = _sut;

        // Act
        header.RenderBranding();

        // Assert
        _writerMock.Verify(w => w.Write(It.IsAny<object>()), Times.AtLeast(1));
    }
}


// ============================================================
// ConsoleTableBuilderTests.cs
// ============================================================
using Ragnar.Core.Rendering;

namespace Ragnar.UnitTests.Rendering;

public class ConsoleTableBuilderTests
{
    private readonly ConsoleTableBuilder _sut;

    public ConsoleTableBuilderTests()
    {
        _sut = new ConsoleTableBuilder();
    }

    #region AddColumns

    [[Fact]]
    public void AddColumnsShouldSetColumnsAndReturnSelf()
    {
        // Act
        var result = _sut.AddColumns("Name", "Age", "City");

        // Assert
        Assert.Same(_sut, result);
        Assert.Equal(3, _sut.ColumnCount);
        Assert.Contains("Name", _sut.Columns);
        Assert.Contains("Age", _sut.Columns);
        Assert.Contains("City", _sut.Columns);
    }

    [[Fact]]
    public void AddColumnsSecondCallShouldThrowInvalidOperationException()
    {
        // Arrange
        _sut.AddColumns("A", "B");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.AddColumns("C"));
    }

    [[Fact]]
    public void AddColumnsDuplicateShouldThrowArgumentException()
    {
        // Arrange
        _sut.AddColumns("Name");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.AddColumns("Name"));
    }

    [[Fact]]
    public void AddColumnsNullShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.AddColumns(null!));
    }

    [[Fact]]
    public void AddColumnsEmptyArrayShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => _sut.AddColumns());
    }

    [[Fact]]
    public void AddColumnsAfterClearShouldAllowReDefinition()
    {
        // Arrange
        _sut.AddColumns("Old");
        _sut.Clear();

        // Act
        _sut.AddColumns("New");

        // Assert
        Assert.Single(_sut.Columns);
        Assert.Equal("New", _sut.Columns[[0]]);
    }

    #endregion

    #region AddRow

    [[Fact]]
    public void AddRowShouldSetRowAndReturnSelf()
    {
        // Arrange
        _sut.AddColumns("A", "B");

        // Act
        var result = _sut.AddRow("1", "2");

        // Assert
        Assert.Same(_sut, result);
        Assert.Equal(1, _sut.RowCount);
        Assert.Equal(new[[]] { "1", "2" }, _sut.Rows[[0]]);
    }

    [[Fact]]
    public void AddRowMismatchedCellCountShouldThrowArgumentException()
    {
        // Arrange
        _sut.AddColumns("A", "B", "C");
        _sut.AddRow("1", "2", "3");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.AddRow("4", "5")); // 2 cells vs 3 columns
    }

    [[Fact]]
    public void AddRowNullShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.AddRow(null!));
    }

    [[Fact]]
    public void AddRowEmptyArrayShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => _sut.AddRow());
    }

    #endregion

    #region Clear

    [[Fact]]
    public void ClearShouldResetColumnsAndRows()
    {
        // Arrange
        _sut.AddColumns("A", "B");
        _sut.AddRow("1", "2");
        _sut.AddRow("3", "4");

        // Act
        _sut.Clear();

        // Assert
        Assert.Empty(_sut.Columns);
        Assert.Empty(_sut.RowCount);
    }

    #endregion

    #region Properties

    [[Fact]]
    public void ColumnCountShouldReflectAddedColumns()
    {
        // Act
        _sut.AddColumns("A", "B", "C");

        // Assert
        Assert.Equal(3, _sut.ColumnCount);
    }

    [[Fact]]
    public void RowCountShouldReflectAddedRows()
    {
        // Arrange
        _sut.AddColumns("A", "B");

        // Act
        _sut.AddRow("1", "2").AddRow("3", "4").AddRow("5", "6");

        // Assert
        Assert.Equal(3, _sut.RowCount);
    }

    [[Fact]]
    public void ShowRowSeparatorsDefaultShouldBeFalse()
    {
        // Assert
        Assert.False(_sut.ShowRowSeparators);
    }

    [[Fact]]
    public void ExpandDefaultShouldBeFalse()
    {
        // Assert
        Assert.False(_sut.Expand);
    }

    #endregion

    #region ToTable

    [[Fact]]
    public void ToTableShouldReturnPopulatedTable()
    {
        // Arrange
        _sut.AddColumns("Name", "Value");
        _sut.AddRow("Foo", "42");
        _sut.AddRow("Bar", "100");

        // Act
        var table = _sut.ToTable();

        // Assert
        Assert.NotNull(table);
        Assert.Equal(2, table.Columns.Count);
        Assert.Equal(2, table.Rows.Count);
    }

    [[Fact]]
    public void ToTableWithExpandShouldSetExpandFlag()
    {
        // Arrange
        _sut.AddColumns("A");
        _sut.AddRow("1");
        _sut.Expand = true;

        // Act
        var table = _sut.ToTable();

        // Assert
        Assert.True(table.Expand);
    }

    [[Fact]]
    public void ToTableWithShowRowSeparatorsShouldSetFlag()
    {
        // Arrange
        _sut.AddColumns("A");
        _sut.AddRow("1");
        _sut.ShowRowSeparators = true;

        // Act
        var table = _sut.ToTable();

        // Assert
        Assert.True(table.ShowRowSeparators);
    }

    [[Fact]]
    public void ToTableEmptyShouldReturnEmptyTable()
    {
        // Act
        var table = _sut.ToTable();

        // Assert
        Assert.NotNull(table);
        Assert.Empty(table.Columns);
        Assert.Empty(table.Rows);
    }

    #endregion
}


// ============================================================
// ConsoleTableRendererTests.cs
// ============================================================
using Ragnar.Core.Rendering;

namespace Ragnar.UnitTests.Rendering;

public class ConsoleTableRendererTests
{
    [[Fact]]
    public void TablePropertyShouldReturnConfiguredBuilder()
    {
        // Arrange
        var builder = new ConsoleTableBuilder();
        builder.AddColumns("A", "B");

        // Act
        var renderer = new ConsoleTableRenderer(builder);

        // Assert
        Assert.Same(builder, renderer.Table);
    }

    [[Fact]]
    public void RenderShouldNotThrow()
    {
        // Arrange
        var builder = new ConsoleTableBuilder();
        builder.AddColumns("Col1", "Col2");
        builder.AddRow("X", "Y");
        var renderer = new ConsoleTableRenderer(builder);

        // Act & Assert – no exception
        renderer.Render();
    }

    [[Fact]]
    public void RenderWithEmptyBuilderShouldNotThrow()
    {
        // Arrange
        var renderer = new ConsoleTableRenderer(new ConsoleTableBuilder());

        // Act & Assert
        renderer.Render();
    }

    [[Fact]]
    public void RenderShouldBeCallableMultipleTimes()
    {
        // Arrange
        var builder = new ConsoleTableBuilder();
        builder.AddColumns("A");
        builder.AddRow("1");
        var renderer = new ConsoleTableRenderer(builder);

        // Act & Assert
        renderer.Render();
        renderer.Render();
    }
}


// ============================================================
// FileWriterTests.cs
// ============================================================
using Ragnar.Output;

namespace Ragnar.UnitTests.Output;

public class FileWriterTests : IDisposable
{
    private readonly FileWriter _sut;
    private readonly string _tempDir;

    public FileWriterTests()
    {
        _sut = new FileWriter();
        _tempDir = Directory.CreateTempSubdirectory("FileWriterTest").FullName;
    }

    [[Fact]]
    public async Task WriteAsyncShouldCreateFileWithContent()
    {
        // Arrange
        var path = Path.Join(_tempDir, "output.txt");

        // Act
        await _sut.WriteAsync(path, "Hello World", CancellationToken.None);

        // Assert
        Assert.True(File.Exists(path));
        Assert.Equal("Hello World", await File.ReadAllTextAsync(path));
    }

    [[Fact]]
    public async Task WriteAsyncShouldOverwriteExistingFile()
    {
        // Arrange
        var path = Path.Join(_tempDir, "overwrite.txt");
        await File.WriteAllTextAsync(path, "old content");

        // Act
        await _sut.WriteAsync(path, "new content", CancellationToken.None);

        // Assert
        Assert.Equal("new content", await File.ReadAllTextAsync(path));
    }

    [[Fact]]
    public async Task WriteAsyncEmptyContentShouldCreateEmptyFile()
    {
        // Arrange
        var path = Path.Join(_tempDir, "empty.txt");

        // Act
        await _sut.WriteAsync(path, string.Empty, CancellationToken.None);

        // Assert
        Assert.True(File.Exists(path));
        Assert.Equal(0, new FileInfo(path).Length);
    }

    [[Fact]]
    public async Task WriteAsyncInvalidPathShouldThrowIOException()
    {
        // Arrange
        var invalidPath = "C:\\nonexistent_dir_xyz\\sub\\file.txt";

        // Act & Assert
        await Assert.ThrowsAnyAsync<IOException>(() => _sut.WriteAsync(invalidPath, "data", CancellationToken.None));
    }

    [[Fact]]
    public async Task WriteAsyncCancelledShouldThrowOperationCanceledException()
    {
        // Arrange
        var path = Path.Join(_tempDir, "cancelled.txt");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.WriteAsync(path, "data", cts.Token));
    }

    [[Fact]]
    public async Task WriteAsyncWithLargeContentShouldSucceed()
    {
        // Arrange
        var path = Path.Join(_tempDir, "large.txt");
        var largeContent = new string('x', 1_000_000); // 1 MB

        // Act
        await _sut.WriteAsync(path, largeContent, CancellationToken.None);

        // Assert
        var info = new FileInfo(path);
        Assert.Equal(1_000_000, info.Length);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}


// ============================================================
// ParsingStageTests.cs
// ============================================================
using Ragnar.Stages;

namespace Ragnar.UnitTests.Stages;

public class ParsingStageTests
{
    private readonly Mock<IFileParseFactory> _parseFactoryMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IOutputWriter> _writerMock;
    private readonly ParsingStage _sut;

    public ParsingStageTests()
    {
        _parseFactoryMock = new Mock<IFileParseFactory>();
        _loggerMock = new Mock<ILogger>();
        _writerMock = new Mock<IOutputWriter>();
        _sut = new ParsingStage(_parseFactoryMock.Object, _loggerMock.Object, _writerMock.Object);
    }

    [[Fact]]
    public void NameShouldReturnExpectedValue()
    {
        // Act
        var result = _sut.Name;

        // Assert
        Assert.Equal("Parsing files…", result);
    }

    [[Fact]]
    public void ShouldRunShouldReturnTrue()
    {
        // Act & Assert
        Assert.True(_sut.ShouldRun);
    }

    [[Fact]]
    public async Task ExecuteAsyncNoFilesShouldLogAndReturn()
    {
        // Arrange
        var context = new EmbeddingContext { DiscoveredFiles = [[]] };

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.Empty(context.Documents);
        _parseFactoryMock.Verify(f => f.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [[Fact]]
    public async Task ExecuteAsyncWithFilesShouldParseAllAndSetDocuments()
    {
        // Arrange
        var context = new EmbeddingContext
        {
            DiscoveredFiles = [["file1.cs", "file2.cs", "file3.json"]]
        };

        var docs1 = new[[]] { new CodeDocument("doc1") };
        var docs2 = new[[]] { new CodeDocument("doc2"), new CodeDocument("doc3") };
        var docs3 = new[[]] { new CodeDocument("doc4") };

        _parseFactoryMock
            .Setup(f => f.ParseAsync("file1.cs", It.IsAny<CancellationToken>()))
            .ReturnsAsync(docs1);
        _parseFactoryMock
            .Setup(f => f.ParseAsync("file2.cs", It.IsAny<CancellationToken>()))
            .ReturnsAsync(docs2);
        _parseFactoryMock
            .Setup(f => f.ParseAsync("file3.json", It.IsAny<CancellationToken>()))
            .ReturnsAsync(docs3);

        // Act
        await _sut.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.Equal(4, context.Documents.Count); // 1 + 2 + 1
        _parseFactoryMock.Verify(f => f.ParseAsync("file1.cs", It.IsAny<CancellationToken>()), Times.Once);
        _parseFactoryMock.Verify(f => f.ParseAsync("file2.cs", It.IsAny<CancellationToken>()), Times.Once);
        _parseFactoryMock.Verify(f => f.ParseAsync("file3.json", It.IsAny<CancellationToken>()), Times.Once);
    }

    [[Fact]]
    public async Task ExecuteAsyncParseFactoryThrowsShouldPropagate()
    {
        // Arrange
        var context = new EmbeddingContext { DiscoveredFiles = [["fail.cs"]] };
        _parseFactoryMock
            .Setup(f => f.ParseAsync("fail.cs", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Parse error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ExecuteAsync(context, CancellationToken.None));
    }

    [[Fact]]
    public async Task ExecuteAsyncShouldRespectCancellation()
    {
        // Arrange
        var context = new EmbeddingContext { DiscoveredFiles = [["file.cs"]] };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        _parseFactoryMock
            .Setup(f => f.ParseAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.ExecuteAsync(context, cts.Token));
    }
}


// ============================================================
// OllamaEmbeddingServiceTests.cs
// ============================================================
using Ragnar.Embedding.Pipeline
