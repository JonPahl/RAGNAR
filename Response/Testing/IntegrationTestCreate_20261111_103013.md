### <span style="color:darkblue;">Testing</span> Please create or modify integration tests using (xunit and moq) for all public methods where applicable.
> **Date Generated**: 11/11/2026 4:30:13 AM
> ## Question: 
> Please create or modify integration tests using (xunit and moq) for all public methods where applicable.
> **Method Call Duration**: 01:37:14
 ## Response: 
```csharp
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Polly;
using Ragnar.Core;
using Ragnar.Options;
using Ragnar.Pipeline;
using Ragnar.Pipeline.Stages;
using Ragnar.Question;
using Ragnar.Services;
using Xunit;
using Lock = System.Threading.Lock;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using IFileParser = Ragnar.Parsing.IFileParser;
using IPipeline = Ragnar.Pipeline.IPipeline<TContext>;
using IPipelineStage = Ragnar.Pipeline.IPipelineStage<TContext>;
using PipelineRunner = Ragnar.Pipeline.PipelineRunner<TContext>;
using StageExecutionContext = Ragnar.Pipeline.StageExecutionContext<TContext>;
using StageTimingMiddleware = Ragnar.Pipeline.StageTimingMiddleware;
using QuestionFilterSelector = Ragnar.Question.QuestionFilterSelector;
using XmlCommentFilterStrategy = Ragnar.Question.XmlCommentFilterStrategy;
using IFilterStrategy = Ragnar.Question.IFilterStrategy;
using QuestionCategory = Ragnar.Question.QuestionCategory;
using Filter = Qdrant.Client.Models.Filter;
using IFileEligibilityValidator = Ragnar.Services.IFileEligibilityValidator;
using FileDiscoveryService = Ragnar.Services.FileDiscoveryService;
using IFileDiscoveryService = Ragnar.Services.IFileDiscoveryService;
using DeltaEmbedStage = Ragnar.Pipeline.Stages.DeltaEmbedStage;
using IFileFingerprintStore = Ragnar.Services.IFileFingerprintStore;
using IVectorStoreWriter = Ragnar.Services.IVectorStoreWriter;
using JsonFileFingerprintStore = Ragnar.Services.JsonFileFingerprintStore;
using BrandingStage = Ragnar.Pipeline.Stages.BrandingStage;
using IApplicationHeader = Ragnar.Pipeline.Stages.IApplicationHeader;
using OllamaChatClientBuilder = Ragnar.Services.OllamaChatClientBuilder;
using IOllamaChatClientBuilder = Ragnar.Services.IOllamaChatClientBuilder;
using OllamaChatClientBuilderConfigured = Ragnar.Services.OllamaChatClientBuilderConfigured;
using OllamaServiceType = Ragnar.Services.OllamaServiceType;
using OllamaClientOptions = Ragnar.Services.OllamaClientOptions;
using EnumConverter = Ragnar.Question.EnumConverter;
using ICsvConverter = Ragnar.Question.ICsvConverter<QuestionCategory>;
using QdrantFilterBuilder = Ragnar.Question.QdrantFilterBuilder;
using CodeParserFactory = Ragnar.Parsing.CodeParserFactory;
using IFileParseFactory = Ragnar.Parsing.IFileParseFactory;
using MarkdownFileParser = Ragnar.Parsing.MarkdownFileParser;
using FileSystemEntryExtensions = Ragnar.Extensions.FileSystemEntryExtensions;
using AssemblyExtensions = Ragnar.Extensions.AssemblyExtensions;
using QuestionPipelineContext = Ragnar.Question.QuestionPipelineContext;
using QuestionTableStage = Ragnar.Question.QuestionTableStage;
using IOutputWriter = Ragnar.Question.IOutputWriter;
using StoredFileListStage = Ragnar.Pipeline.Stages.StoredFileListStage;
using IQdrantClient = Qdrant.Client.IQdrantClient;
using VectorStoreWriter = Ragnar.Services.VectorStoreWriter;
using IEmbeddingService = Ragnar.Services.IEmbeddingService;
using IQdrantPointFactory = Ragnar.Services.IQdrantPointFactory;
using UpdateStatus = Ragnar.Services.UpdateStatus;
using CodeDocument = Ragnar.Parsing.CodeDocument;
using EmbeddingContext = Ragnar.Pipeline.EmbeddingContext;
using AsyncRetryPolicy = Polly.AsyncRetryPolicy;
using IAsyncPolicy = Polly.IAsyncPolicy;
using PipelineStageException = Ragnar.Pipeline.PipelineStageException;
using PromptFormatting = Ragnar.Extensions.PromptFormatting;
using DomainDefaults = Ragnar.Extensions.DomainDefaults;
using Guard = FluentValidation.Guard;
using CsvReader = CsvHelper.CsvReader;
using CsvWriter = CsvHelper.CsvWriter;
using TextReader = System.IO.TextReader;
using TextWriter = System.IO.TextWriter;
using StageEntry = Ragnar.Pipeline.StageEntry<TContext>;

// ============================================================================
// PipelineRunner Integration Tests
// ============================================================================

public class PipelineRunnerTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly PipelineRunner<object> _runner;

    public PipelineRunnerTests()
    {
        _loggerMock = new Mock<ILogger>();
        _runner = new PipelineRunner<object>(_loggerMock.Object);
    }

    // --- AddStage ---

    [[Fact]]
    public void AddStage_ShouldRegisterStageAndReturnRunner()
    {
        var stage = new Mock<IPipelineStage<object>>();
        stage.Setup(s => s.Name).Returns("TestStage");

        var result = _runner.AddStage(stage.Object);

        Assert.Same(_runner, result);
        Assert.Contains("TestStage", _runner.StageNames);
    }

    [[Fact]]
    public void AddStage_WithNullStage_ShouldThrow()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => _runner.AddStage(null!));
        Assert.Equal("stage", ex.ParamName);
    }

    [[Fact]]
    public void AddStage_WithDuplicateName_ShouldThrow()
    {
        var stage1 = new Mock<IPipelineStage<object>>();
        stage1.Setup(s => s.Name).Returns("Duplicate");
        var stage2 = new Mock<IPipelineStage<object>>();
        stage2.Setup(s => s.Name).Returns("Duplicate");

        _runner.AddStage(stage1.Object);

        Assert.Throws<ArgumentException>(() => _runner.AddStage(stage2.Object));
    }

    [[Fact]]
    public void AddStage_WithRetryPolicy_ShouldStorePolicy()
    {
        var stage = new Mock<IPipelineStage<object>>();
        stage.Setup(s => s.Name).Returns("RetryStage");
        var policy = Policy.WrapAsync(
            Policy.Handle<HttpRequestException>().WaitAndRetryAsync(3));

        _runner.AddStage(stage.Object, policy);

        Assert.Contains("RetryStage", _runner.StageNames);
    }

    // --- StageNames ---

    [[Fact]]
    public void StageNames_ShouldReturnEmptyWhenNoStages()
    {
        Assert.Empty(_runner.StageNames);
    }

    [[Fact]]
    public void StageNames_ShouldReturnAllRegisteredNamesInOrder()
    {
        var s1 = new Mock<IPipelineStage<object>>();
        s1.Setup(x => x.Name).Returns("First");
        var s2 = new Mock<IPipelineStage<object>>();
        s2.Setup(x => x.Name).Returns("Second");
        var s3 = new Mock<IPipelineStage<object>>();
        s3.Setup(x => x.Name).Returns("Third");

        _runner.AddStage(s1.Object).AddStage(s2.Object).AddStage(s3.Object);

        var names = _runner.StageNames;
        Assert.Equal(new[[]] { "First", "Second", "Third" }, names);
    }

    // --- RemoveStage ---

    [[Fact]]
    public void RemoveStage_ShouldRemoveByName()
    {
        var s1 = new Mock<IPipelineStage<object>>();
        s1.Setup(x => x.Name).Returns("ToRemove");
        var s2 = new Mock<IPipelineStage<object>>();
        s2.Setup(x => x.Name).Returns("Keep");

        _runner.AddStage(s1.Object).AddStage(s2.Object);
        _runner.RemoveStage("ToRemove");

        Assert.Single(_runner.StageNames);
        Assert.Equal("Keep", _runner.StageNames[[0]]);
    }

    [[Fact]]
    public void RemoveStage_NonExistentName_ShouldNotThrow()
    {
        _runner.RemoveStage("DoesNotExist");
        Assert.Empty(_runner.StageNames);
    }

    // --- ClearStages ---

    [[Fact]]
    public void ClearStages_ShouldRemoveAll()
    {
        var s1 = new Mock<IPipelineStage<object>>();
        s1.Setup(x => x.Name).Returns("A");
        var s2 = new Mock<IPipelineStage<object>>();
        s2.Setup(x => x.Name).Returns("B");

        _runner.AddStage(s1.Object).AddStage(s2.Object);
        var result = _runner.ClearStages();

        Assert.Same(_runner, result);
        Assert.Empty(_runner.StageNames);
    }

    // --- Reorder ---

    [[Fact]]
    public void Reorder_ShouldMoveStageToNewIndex()
    {
        var s1 = new Mock<IPipelineStage<object>>();
        s1.Setup(x => x.Name).Returns("A");
        var s2 = new Mock<IPipelineStage<object>>();
        s2.Setup(x => x.Name).Returns("B");
        var s3 = new Mock<IPipelineStage<object>>();
        s3.Setup(x => x.Name).Returns("C");

        _runner.AddStage(s1.Object).AddStage(s2.Object).AddStage(s3.Object);
        _runner.Reorder("C", 0);

        Assert.Equal(new[[]] { "C", "A", "B" }, _runner.StageNames);
    }

    [[Fact]]
    public void Reorder_NonExistentName_ShouldThrow()
    {
        Assert.Throws<KeyNotFoundException>(() => _runner.Reorder("Ghost", 0));
    }

    [[Fact]]
    public void Reorder_IndexOutOfRange_ShouldClamp()
    {
        var s1 = new Mock<IPipelineStage<object>>();
        s1.Setup(x => x.Name).Returns("A");
        var s2 = new Mock<IPipelineStage<object>>();
        s2.Setup(x => x.Name).Returns("B");

        _runner.AddStage(s1.Object).AddStage(s2.Object);
        _runner.Reorder("A", 99);

        // Clamped to last position (index 1)
        Assert.Equal(new[[]] { "B", "A" }, _runner.StageNames);
    }

    // --- ReplaceStage ---

    [[Fact]]
    public void ReplaceStage_ShouldReplaceAtSamePosition()
    {
        var old = new Mock<IPipelineStage<object>>();
        old.Setup(x => x.Name).Returns("Old");
        var keep = new Mock<IPipelineStage<object>>();
        keep.Setup(x => x.Name).Returns("Keep");

        _runner.AddStage(keep.Object).AddStage(old.Object);

        var replacement = new Mock<IPipelineStage<object>>();
        replacement.Setup(x => x.Name).Returns("New");

        _runner.ReplaceStage("Old", replacement.Object);

        Assert.Equal(new[[]] { "Keep", "New" }, _runner.StageNames);
    }

    [[Fact]]
    public void ReplaceStage_NonExistent_ShouldThrow()
    {
        var replacement = new Mock<IPipelineStage<object>>();
        replacement.Setup(x => x.Name).Returns("New");

        Assert.Throws<KeyNotFoundException>(() => _runner.ReplaceStage("Ghost", replacement.Object));
    }

    // --- UseMiddleware / UseTiming ---

    [[Fact]]
    public void UseMiddleware_ShouldRegisterAndReturnRunner()
    {
        var mw = (StageExecutionContext<object> ctx, Func<Task> next, CancellationToken ct) =>
            next();

        var result = _runner.UseMiddleware(mw);

        Assert.Same(_runner, result);
    }

    [[Fact]]
    public void UseMiddleware_WithNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => _runner.UseMiddleware(null!));
    }

    [[Fact]]
    public void UseTiming_ShouldRegisterTimingMiddleware()
    {
        var timing = new StageTimingMiddleware(_loggerMock.Object);
        var result = _runner.UseTiming(timing);

        Assert.Same(_runner, result);
    }

    // --- ExecuteAsync ---

    [[Fact]]
    public async Task ExecuteAsync_NoStages_ShouldReturnContext()
    {
        var ctx = new object();
        var result = await _runner.ExecuteAsync(ctx, CancellationToken.None);

        Assert.Same(ctx, result);
    }

    [[Fact]]
    public async Task ExecuteAsync_ShouldCallStagesInOrder()
    {
        var callOrder = new List<string>();

        var s1 = CreateStage("S1", callOrder);
        var s2 = CreateStage("S2", callOrder);

        _runner.AddStage(s1).AddStage(s2);
        await _runner.ExecuteAsync(new object(), CancellationToken.None);

        Assert.Equal(new[[]] { "S1", "S2" }, callOrder);
    }

    [[Fact]]
    public async Task ExecuteAsync_SkippedStage_ShouldNotExecute()
    {
        var s1 = new Mock<IPipelineStage<object>>();
        s1.Setup(x => x.Name).Returns("SkipMe");
        s1.Setup(x => x.ShouldRun(It.IsAny<object>())).Returns(false);
        s1.Setup(x => x.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask);

        _runner.AddStage(s1.Object);
        await _runner.ExecuteAsync(new object(), CancellationToken.None);

        s1.Verify(x => x.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [[Fact]]
    public async Task ExecuteAsync_WithCancelledToken_ShouldThrow()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var s1 = new Mock<IPipelineStage<object>>();
        s1.Setup(x => x.Name).Returns("Cancelled");
        s1.Setup(x => x.ShouldRun(It.IsAny<object>())).Returns(true);
        s1.Setup(x => x.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
          .Throws(new OperationCanceledException());

        _runner.AddStage(s1.Object);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _runner.ExecuteAsync(new object(), cts.Token));
    }

    [[Fact]]
    public async Task ExecuteAsync_StageFails_ShouldThrowPipelineStageException()
    {
        var inner = new InvalidOperationException("boom");
        var s1 = new Mock<IPipelineStage<object>>();
        s1.Setup(x => x.Name).Returns("Failing");
        s1.Setup(x => x.ShouldRun(It.IsAny<object>())).Returns(true);
        s1.Setup(x => x.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
          .Throws(inner);

        _runner.AddStage(s1.Object);

        var ex = await Assert.ThrowsAsync<PipelineStageException>(
            () => _runner.ExecuteAsync(new object(), CancellationToken.None));

        Assert.Equal("Failing", ex.StageName);
        Assert.Same(inner, ex.InnerException);
    }

    [[Fact]]
    public async Task ExecuteAsync_WithMiddleware_ShouldWrapStage()
    {
        var middlewareRan = false;
        var s1 = new Mock<IPipelineStage<object>>();
        s1.Setup(x => x.Name).Returns("Mid");
        s1.Setup(x => x.ShouldRun(It.IsAny<object>())).Returns(true);
        s1.Setup(x => x.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask);

        _runner.UseMiddleware((ctx, next, ct) =>
        {
            middlewareRan = true;
            return next();
        });

        _runner.AddStage(s1.Object);
        await _runner.ExecuteAsync(new object(), CancellationToken.None);

        Assert.True(middlewareRan);
    }

    [[Fact]]
    public async Task ExecuteAsync_MultipleMiddlewares_ShouldApplyInOrder()
    {
        var order = new List<string>();

        _runner.UseMiddleware((ctx, next, ct) => { order.Add("MW1-before"); return next().ContinueWith(_ => order.Add("MW1-after")); });
        _runner.UseMiddleware((ctx, next, ct) => { order.Add("MW2-before"); return next().ContinueWith(_ => order.Add("MW2-after")); });

        var s1 = new Mock<IPipelineStage<object>>();
        s1.Setup(x => x.Name).Returns("Core");
        s1.Setup(x => x.ShouldRun(It.IsAny<object>())).Returns(true);
        s1.Setup(x => x.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask);

        _runner.AddStage(s1.Object);
        await _runner.ExecuteAsync(new object(), CancellationToken.None);

        // First registered middleware is outermost
        Assert.Contains("MW1-before", order);
        Assert.Contains("MW2-before", order);
        Assert.Contains("Core", order);
        Assert.Contains("MW2-after", order);
        Assert.Contains("MW1-after", order);
        Assert.Equal(0, order.IndexOf("MW1-before"));
        Assert.Greater(order.IndexOf("MW2-before"), order.IndexOf("MW1-before"));
    }

    [[Fact]]
    public async Task ExecuteAsync_WithRetryPolicy_ShouldRetryOnFailure()
    {
        var attempts = 0;
        var s1 = new Mock<IPipelineStage<object>>();
        s1.Setup(x => x.Name).Returns("Retryable");
        s1.Setup(x => x.ShouldRun(It.IsAny<object>())).Returns(true);
        s1.SetupSequence(x => x.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
          .Throws(new HttpRequestException("fail1"))
          .Throws(new HttpRequestException("fail2"))
          .Returns(Task.CompletedTask);

        var policy = Policy.Handle<HttpRequestException>()
            .WaitAndRetryAsync(3);

        _runner.AddStage(s1.Object, policy);
        await _runner.ExecuteAsync(new object(), CancellationToken.None);

        s1.Verify(x => x.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    private static IPipelineStage<object> CreateStage(string name, List<string> callOrder)
    {
        var mock = new Mock<IPipelineStage<object>>();
        mock.Setup(x => x.Name).Returns(name);
        mock.Setup(x => x.ShouldRun(It.IsAny<object>())).Returns(true);
        mock.Setup(x => x.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add(name))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }
}

// ============================================================================
// FileDiscoveryService Integration Tests
// ============================================================================

public class FileDiscoveryServiceTests : IDisposable
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IFileEligibilityValidator> _validatorMock;
    private readonly IFileDiscoveryService _sut;
    private readonly string _tempDir;

    public FileDiscoveryServiceTests()
    {
        _loggerMock = new Mock<ILogger>();
        _validatorMock = new Mock<IFileEligibilityValidator>();
        _validatorMock.Setup(v => v.IsValid(It.IsAny<FileInfo>(), It.IsAny<FileLoadOptions>()))
            .Returns(true);

        var config = new RagnarConfig
        {
            FileLoadOptions = new FileLoadOptions
            {
                AllowedExtensions = new List<string> { ".cs", ".json" }
            },
            ApplicationOptions = new ApplicationOptions
            {
                VectorStoreName = "test",
                SourceDirectory = ""
            },
            EmbeddingOptions = new EmbeddingOptions
            {
                Dimension = 768,
                EmbeddingModel = "model",
                Host = new Uri("http://localhost:11434"),
                Timeout = TimeSpan.FromSeconds(30),
                BatchSize = 16
            },
            GenerationOptions = new GenerationOptions(),
            OllamaOptions = new OllamaOptions
            {
                Host = new Uri("http://localhost:11434"),
                LlmModel = "llm",
                Timeout = TimeSpan.FromSeconds(30)
            }
        };

        _sut = new FileDiscoveryService(_loggerMock.Object, _validatorMock.Object);
        _tempDir = Path.Combine(Path.GetTempPath(), $"ragnar_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [[Fact]]
    public async Task GetFilesAsync_ShouldYieldEligibleFiles()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_tempDir, "one.cs"), "// test");
        File.WriteAllText(Path.Combine(_tempDir, "two.json"), "{}");
        File.WriteAllText(Path.Combine(_tempDir, "skip.txt"), "nope");

        var options = Options.Create(BuildConfig());
        _validatorMock.Setup(v => v.IsValid(It.IsAny<FileInfo>(), It.IsAny<FileLoadOptions>()))
            .Returns((FileInfo fi, FileLoadOptions _) => fi.Extension is ".cs" or ".json");

        // Act
        var results = new List<string>();
        await foreach (var f in _sut.GetFilesAsync(_tempDir, options, CancellationToken.None))
            results.Add(f);

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(r => r.EndsWith("one.cs"), results);
        Assert.Contains(r => r.EndsWith("two.json"), results);
    }

    [[Fact]]
    public async Task GetFilesAsync_NonExistentDirectory_ShouldThrow()
    {
        var options = Options.Create(BuildConfig());

        await Assert.ThrowsAsync<DirectoryNotFoundException>(
            async () => await _sut.GetFilesAsync("/nonexistent/path/xyz", options, CancellationToken.None).ToListAsync());
    }

    [[Fact]]
    public async Task GetFilesAsync_EmptyDirectory_ShouldYieldNothing()
    {
        var emptyDir = Path.Combine(_tempDir, "empty");
        Directory.CreateDirectory(emptyDir);
        var options = Options.Create(BuildConfig());

        var results = new List<string>();
        await foreach (var f in _sut.GetFilesAsync(emptyDir, options, CancellationToken.None))
            results.Add(f);

        Assert.Empty(results);
    }

    [[Fact]]
    public async Task GetFilesAsync_WithCancellation_ShouldThrow()
    {
        File.WriteAllText(Path.Combine(_tempDir, "c1.cs"), "//");
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var options = Options.Create(BuildConfig());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await _sut.GetFilesAsync(_tempDir, options, cts.Token).ToListAsync());
    }

    [[Fact]]
    public async Task GetFilesAsync_WithSubdirectories_ShouldFindNestedFiles()
    {
        var sub = Path.Combine(_tempDir, "sub", "deep");
        Directory.CreateDirectory(sub);
        File.WriteAllText(Path.Combine(sub, "nested.cs"), "// nested");

        var options = Options.Create(BuildConfig());
        _validatorMock.Setup(v => v.IsValid(It.IsAny<FileInfo>(), It.IsAny<FileLoadOptions>()))
            .Returns(true);

        var results = new List<string>();
        await foreach (var f in _sut.GetFilesAsync(_tempDir, options, CancellationToken.None))
            results.Add(f);

        Assert.Contains(r => r.Contains("nested.cs"), results);
    }

    [[Fact]]
    public async Task GetFilesAsync_AllFilesSkipped_ShouldYieldNothing()
    {
        File.WriteAllText(Path.Combine(_tempDir, "skip.cs"), "//");
        var options = Options.Create(BuildConfig());
        _validatorMock.Setup(v => v.IsValid(It.IsAny<FileInfo>(), It.IsAny<FileLoadOptions>()))
            .Returns(false);

        var results = new List<string>();
        await foreach (var f in _sut.GetFilesAsync(_tempDir, options, CancellationToken.None))
            results.Add(f);

        Assert.Empty(results);
    }

    private static RagnarConfig BuildConfig() => new()
    {
        FileLoadOptions = new FileLoadOptions
        {
            AllowedExtensions = new List<string> { ".cs" }
        },
        ApplicationOptions = new ApplicationOptions
        {
            VectorStoreName = "test",
            SourceDirectory = ""
        },
        EmbeddingOptions = new EmbeddingOptions
        {
            Dimension = 768,
            EmbeddingModel = "m",
            Host = new Uri("http://localhost:11434"),
            Timeout = TimeSpan.FromSeconds(30),
            BatchSize = 16
        },
        GenerationOptions = new GenerationOptions(),
        OllamaOptions = new OllamaOptions
        {
            Host = new Uri("http://localhost:11434"),
            LlmModel = "llm",
            Timeout = TimeSpan.FromSeconds(30)
        }
    };
}

// ============================================================================
// DeltaEmbedStage Integration Tests
// ============================================================================

public class DeltaEmbedStageTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IFileFingerprintStore> _fpStoreMock;
    private readonly Mock<IVectorStoreWriter> _writerMock;
    private readonly IOptions<RagnarConfig> _options;
    private readonly DeltaEmbedStage _sut;

    public DeltaEmbedStageTests()
    {
        _loggerMock = new Mock<ILogger>();
        _fpStoreMock = new Mock<IFileFingerprintStore>();
        _writerMock = new Mock<IVectorStoreWriter>();

        var config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions { VectorStoreName = "test", SourceDirectory = "" },
            EmbeddingOptions = new EmbeddingOptions
            {
                Dimension = 768,
                EmbeddingModel = "m",
                Host = new Uri("http://localhost:11434"),
                Timeout = TimeSpan.FromSeconds(30),
                BatchSize = 16
            },
            GenerationOptions = new GenerationOptions(),
            OllamaOptions = new OllamaOptions
            {
                Host = new Uri("http://localhost:11434"),
                LlmModel = "llm",
                Timeout = TimeSpan.FromSeconds(30)
            },
            FileLoadOptions = new FileLoadOptions()
        };
        _options = Options.Create(config);
        _sut = new DeltaEmbedStage(_loggerMock.Object, _fpStoreMock.Object, _writerMock.Object, _options);
    }

    [[Fact]]
    public void Name_ShouldBeDeltaEmbed()
    {
        Assert.Equal("Delta Embed", _sut.Name);
    }

    [[Fact]]
    public void RetryPolicy_ShouldNotBeNull()
    {
        Assert.NotNull(_sut.RetryPolicy);
    }

    [[Fact]]
    public void ShouldRun_WithDocuments_ShouldReturnTrue()
    {
        var ctx = new EmbeddingContext { Documents = new List<CodeDocument> { new() { Code = "x" } } };
        Assert.True(_sut.ShouldRun(ctx));
    }

    [[Fact]]
    public void ShouldRun_WithNullDocuments_ShouldReturnFalse()
    {
        var ctx = new EmbeddingContext { Documents = null };
        Assert.False(_sut.ShouldRun(ctx));
    }

    [[Fact]]
    public void ShouldRun_WithEmptyDocuments_ShouldReturnFalse()
    {
        var ctx = new EmbeddingContext { Documents = new List<CodeDocument>() };
        Assert.False(_sut.ShouldRun(ctx));
    }

    [[Fact]]
    public async Task ExecuteAsync_AllFilesUnchanged_ShouldSkipUpsert()
    {
        var doc = new CodeDocument { Code = "code", FilePath = "/test.cs" };
        var ctx = new EmbeddingContext { Documents = new List<CodeDocument> { doc } };

        _fpStoreMock.Setup(s => s.IsUnchangedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.ExecuteAsync(ctx, CancellationToken.None);

        _fpStoreMock.Verify(s => s.InitializeAsync(It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(s => s.UpsertBatchAsync(It.IsAny<IEnumerable<CodeDocument>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [[Fact]]
    public async Task ExecuteAsync_AllFilesChanged_ShouldUpsert()
    {
        var doc = new CodeDocument { Code = "code", FilePath = "/test.cs" };
        var ctx = new EmbeddingContext { Documents = new List<CodeDocument> { doc } };

        _fpStoreMock.Setup(s => s.IsUnchangedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _writerMock.Setup(w => w.UpsertBatchAsync(It.IsAny<IEnumerable<CodeDocument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new() { Status = UpdateStatus.Completed, CalculateSize = 1 });

        await _sut.ExecuteAsync(ctx, CancellationToken.None);

        _writerMock.Verify(s => s.UpsertBatchAsync(It.Is<IEnumerable<CodeDocument>>(d => d.Count() == 1), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [[Fact]]
    public async Task ExecuteAsync_MixedFiles_ShouldUpsertOnlyChanged()
    {
        var changed = new CodeDocument { Code = "new", FilePath = "/changed.cs" };
        var unchanged = new CodeDocument { Code = "same", FilePath = "/same.cs" };
        var ctx = new EmbeddingContext { Documents = new List<CodeDocument> { changed, unchanged } };

        _fpStoreMock.Setup(s => s.IsUnchangedAsync("/changed.cs", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _fpStoreMock.Setup(s => s.IsUnchangedAsync("/same.cs", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _writerMock.Setup(w => w.UpsertBatchAsync(It.IsAny<IEnumerable<CodeDocument>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new() { Status = UpdateStatus.Completed, CalculateSize = 1 });

        await _sut.ExecuteAsync(ctx, CancellationToken.None);

        _writerMock.Verify(s => s.UpsertBatchAsync(
            It.Is<IEnumerable<CodeDocument>>(d => d.Count() == 1 && d.First().FilePath == "/changed.cs"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [[Fact]]
    public async Task ExecuteAsync_NullDocuments_ShouldThrow()
    {
        var ctx = new EmbeddingContext { Documents = null };

        await Assert.Throws<ArgumentNullException>(() => _sut.ExecuteAsync(ctx, CancellationToken.None));
    }

    [[Fact]]
    public async Task ExecuteAsync_CancellationRequested_ShouldThrow()
    {
        var doc = new CodeDocument { Code = "c", FilePath = "/f.cs" };
        var ctx = new EmbeddingContext { Documents = new List<CodeDocument> { doc } };

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.ExecuteAsync(ctx, cts.Token));
    }
}

// ============================================================================
// BrandingStage Integration Tests
// ============================================================================

public class BrandingStageTests
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<IApplicationHeader> _headerMock;
    private readonly BrandingStage _sut;

    public BrandingStageTests()
    {
        _loggerMock = new Mock<ILogger>();
        _headerMock = new Mock<IApplicationHeader>();
        _sut = new BrandingStage(_loggerMock.Object, _headerMock.Object);
    }

    [[Fact]]
    public void Name_ShouldBeBrandingStage()
    {
        Assert.Equal("Branding Stage", _sut.Name);
    }

    [[Fact]]
    public void RetryPolicy_ShouldBeNull()
    {
        Assert.Null(_sut.RetryPolicy);
    }

    [[Fact]]
    public void ShouldRun_ShouldAlwaysReturnTrue()
    {
        var ctx = new EmbeddingContext();
        Assert.True(_sut.ShouldRun(ctx));
        Assert.True(_sut.ShouldRun(null!));
    }

    [[Fact]]
    public async Task ExecuteAsync_ShouldCallRenderBranding()
    {
        var ctx = new EmbeddingContext();

        await _sut.ExecuteAsync(ctx, CancellationToken.None);

        _headerMock.Verify(h => h.RenderBranding(), Times.Once);
    }

    [[Fact]]
    public async Task ExecuteAsync_WithNullContext_ShouldStillRender()
    {
        await _sut.ExecuteAsync(null!, CancellationToken.None);

        _headerMock.Verify(h => h.RenderBranding(), Times.Once);
    }

    [[Fact]]
    public async Task ExecuteAsync_ShouldCompleteWithoutException()
    {
        var ctx = new EmbeddingContext();
        var task = _sut.ExecuteAsync(ctx, CancellationToken.None);

        await task;
        Assert.True(task.IsCompletedSuccessfully);
    }
}

// ============================================================================
// JsonFileFingerprintStore Integration Tests
// ============================================================================

public class JsonFileFingerprintStoreTests : IDisposable
{
    private readonly Mock<ILogger> _loggerMock;
    private readonly IOptions<RagnarConfig> _options;
    private readonly JsonFileFingerprintStore _sut;
    private readonly string _tempDir;

    public JsonFileFingerprintStoreTests()
    {
        _loggerMock = new Mock<ILogger>();
        _tempDir = Path.Combine(Path.GetTempPath(), $"ragnar_fp_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        var config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions { SourceDirectory = _tempDir },
            FileLoadOptions = new FileLoadOptions(),
            EmbeddingOptions = new EmbeddingOptions
            {
                Dimension = 768,
                EmbeddingModel = "m",
                Host = new Uri("http://localhost:11434"),
                Timeout = TimeSpan.FromSeconds(30),
                BatchSize = 16
            },
            GenerationOptions = new GenerationOptions(),
            OllamaOptions = new OllamaOptions
            {
                Host = new Uri("http://localhost:11434"),
                LlmModel = "llm",
                Timeout = TimeSpan.FromSeconds(30)
            }
        };
        _options = Options.Create(config);
        _sut = new JsonFileFingerprintStore(_loggerMock.Object, _options);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [[Fact]]
    public async Task InitializeAsync_ShouldCreateDirectoryIfMissing()
    {
        var nested = Path.Combine(_tempDir, "nested", "dir");
        var config = new RagnarConfig
        {
            ApplicationOptions = new ApplicationOptions { SourceDirectory = nested },
            FileLoadOptions = new FileLoadOptions(),
            EmbeddingOptions = new EmbeddingOptions
            {
                Dimension = 768,
                EmbeddingModel = "m",
                Host = new Uri("http://localhost:11434"),
                Timeout = TimeSpan.FromSeconds(30),
                BatchSize = 16
            },
            GenerationOptions = new GenerationOptions(),
            OllamaOptions = new OllamaOptions
            {
                Host = new Uri("http://localhost:11434"),
                LlmModel = "llm",
                Timeout = TimeSpan.FromSeconds(30)
            }
        };

        var sut = new JsonFileFingerprintStore(_loggerMock.Object, Options.Create(config));
        await sut.InitializeAsync(CancellationToken.None);

        Assert.True(Directory.Exists(nested));
    }

    [[Fact]]
    public async Task InitializeAsync_NoExistingFile_ShouldStartFresh()
    {
        await _sut.InitializeAsync(CancellationToken.None);

        // No exception, store is ready
        var unchanged = await _sut.IsUnchangedAsync("/any.cs", "hash123", CancellationToken.None);
        Assert.False(unchanged);
    }

    [[Fact]]
    public async Task InitializeAsync_ShouldLoadExistingFingerprints()
    {
        await _sut.InitializeAsync(CancellationToken.None);
        await _sut.RecordAsync("/a.cs", "hashA", CancellationToken.None);
        await _sut.FlushAsync(CancellationToken.None);

        // Create a new instance to simulate "next run"
        var sut2 = new JsonFileFingerprintStore(_loggerMock.Object, _options);
        await sut2.InitializeAsync(CancellationToken.None);

        var unchanged = await sut2.IsUnchangedAsync("/a.cs", "hashA", CancellationToken.None);
        Assert.True(unchanged);
    }

    [[Fact]]
    public async Task IsUnchangedAsync_NewFile_ShouldReturnFalse()
    {
        await _sut.InitializeAsync(CancellationToken.None);

        var result = await _sut.IsUnchangedAsync("/new.cs", "abc123", CancellationToken.None);

        Assert.False(result);
    }

    [[Fact]]
    public async Task IsUnchangedAsync_MatchingHash_ShouldReturnTrue()
    {
        await _sut.InitializeAsync(CancellationToken.None);
        await _sut.RecordAsync("/file.cs", "xyz789", CancellationToken.None);

        var result = await _sut.IsUnchangedAsync("/file.cs", "xyz789", CancellationToken.None);

        Assert.True(result);
    }

    [[Fact]]
    public async Task IsUnchangedAsync_DifferentHash_ShouldReturnFalse()
    {
        await _sut.InitializeAsync(CancellationToken.None);
        await _sut.RecordAsync("/file.cs", "old_hash", CancellationToken.None);

        var result = await _sut.IsUnchangedAsync("/file.cs", "new_hash", CancellationToken.None);

        Assert.False(result);
    }

    [[Fact]]
    public async Task IsUnchangedAsync_CancelledToken_ShouldThrow()
    {
        await _sut.InitializeAsync(CancellationToken.None);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _sut.IsUnchangedAsync("/f.cs", "h", cts.Token));
    }

    [[Fact]]
    public async Task RecordAsync_Sh
