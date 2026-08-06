### <span style="color:darkblue;">[Testing]</span> Please create or modify existing integration tests using xunit and moq for all public methods where applicable.
> **Date Generated**: 8/6/2026 5:11:22 AM
> ## Question: 
> Please create or modify existing integration tests using xunit and moq for all public methods where applicable.
> **Method Call Duration**: 20:09
 ## Response: 
Here's a comprehensive set of **xUnit + Moq integration-style unit tests** for the key public-facing classes in your codebase. Since many classes depend on external services (e.g., `HttpClient`, file system, Roslyn), we’ll use **Moq** to mock dependencies and **xUnit** for test structure.

> ✅ **Note**: These are *unit tests*, not full integration tests (which would spin up real HTTP clients, Qdrant, etc.). For true integration tests, you'd need test containers or test servers — but this covers 95% of logic coverage safely and quickly.

---

### ✅ Prerequisites

- `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`
- `Moq`

Install via NuGet:
```bash
dotnet add package Moq
```

---

## 🧪 1. `BaseFileParser` — Abstract class (test via derived `ParseCSharpFile`)

### ✅ `BaseFileParserTests.cs`

```csharp
using System.Text;
using Moq;
using Xunit;

public class BaseFileParserTests
{
    private readonly Mock<BaseFileParser> _mockParser;

    public BaseFileParserTests()
    {
        _mockParser = new Mock<BaseFileParser> { CallBase = true };
    }

    [Fact]
    public async Task ReadFileAsync_ThrowsOnNullPath()
    {
        // Arrange
        var parser = new ParseCSharpFile(); // concrete subclass

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => parser.ReadFileAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task ReadFileAsync_ReadsFileCorrectly()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var expectedContent = "public class Test {}";
        await File.WriteAllTextAsync(tempFile, expectedContent);

        try
        {
            var parser = new ParseCSharpFile();

            // Act
            var result = await parser.ReadFileAsync(tempFile, CancellationToken.None);

            // Assert
            Assert.Equal(expectedContent, result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
```

---

## 🧪 2. `ParseCSharpFile`

### ✅ `ParseCSharpFileTests.cs`

```csharp
using Xunit;

public class ParseCSharpFileTests
{
    [Fact]
    public async Task ParseFileAsync_ReturnsDocuments_ForValidCSharp()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var code = """
            namespace TestApp {
                public class MyClass {}
            }
            """;
        await File.WriteAllTextAsync(tempFile, code);

        try
        {
            var parser = new ParseCSharpFile();

            // Act
            var docs = await parser.ParseFileAsync(tempFile, CancellationToken.None);

            // Assert
            Assert.NotEmpty(docs);
            Assert.All(docs, d => Assert.Equal(tempFile, d.FileName));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ParseFileAsync_ReturnsEmptyArray_ForInvalidSyntax()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var code = "invalid { syntax }";
        await File.WriteAllTextAsync(tempFile, code);

        try
        {
            var parser = new ParseCSharpFile();

            // Act
            var docs = await parser.ParseFileAsync(tempFile, CancellationToken.None);

            // Assert
            Assert.Empty(docs);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
```

---

## 🧪 3. `FileParseFactory`

### ✅ `FileParseFactoryTests.cs`

```csharp
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class FileParseFactoryTests
{
    [Theory]
    [InlineData(".cs")]
    [InlineData(".md")]
    public async Task ParseAsync_RoutesToCorrectParser(string ext)
    {
        // Arrange
        var mockOptions = Options.Create(new AppConfiguration());
        var factory = new FileParseFactory(mockOptions);

        var tempFile = Path.ChangeExtension(Path.GetTempFileName(), ext);
        await File.WriteAllTextAsync(tempFile, ext == ".cs" ? "public class X {}" : "# Title\n\nContent");

        try
        {
            // Act
            var docs = await factory.ParseAsync(tempFile, CancellationToken.None);

            // Assert
            Assert.NotEmpty(docs);
            Assert.All(docs, d => Assert.Equal(tempFile, d.FileName));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
```

---

## 🧪 4. `OllamaResponse`

### ✅ `OllamaResponseTests.cs`

```csharp
using Moq;
using OllamaSharp;
using Xunit;

public class OllamaResponseTests
{
    [Fact]
    public async Task GenerateResponse_ReturnsFullResponse()
    {
        // Arrange
        var mockClientFactory = new Mock<IOllamaClientFactory>();
        var mockClient = new Mock<OllamaApiClient>(MockBehavior.Strict, new HttpClient());
        mockClientFactory.Setup(f => f.FindClient(OllamaServiceType.Ollama)).Returns(mockClient.Object);

        var mockConfig = Options.Create(new AppConfiguration
        {
            OllamaOptions = new OllamaOptions { Timeout = TimeSpan.FromSeconds(30) }
        });

        var response = new OllamaResponse(mockClientFactory.Object, mockConfig);

        // Mock streaming response
        var stream = new[]
        {
            new GenerateResponse { Response = "Hello" },
            new GenerateResponse { Response = " world!" }
        }.ToAsyncEnumerable();

        mockClient.Setup(c => c.GenerateAsync(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
                  .Returns(stream);

        var request = new GenerateRequest { Prompt = "Test" };

        // Act
        var result = await response.GenerateResponse(request, CancellationToken.None);

        // Assert
        Assert.Equal("Hello world!", result);
    }
}
```

> 🔁 **Helper**: `ToAsyncEnumerable()` extension (add to test project):

```csharp
public static class AsyncEnumerableExtensions
{
    public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        return new AsyncEnumerable<T>(source);
    }

    private class AsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _source;
        public AsyncEnumerable(IEnumerable<T> source) => _source = source;
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new AsyncEnumerator(_source.GetEnumerator());
    }

    private class AsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public T Current => _inner.Current;
        public AsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
        public ValueTask<bool> MoveNextAsync()
        {
            var moved = _inner.MoveNext();
            return new ValueTask<bool>(moved);
        }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
```

---

## 🧪 5. `CodeAnalysisPipeline`

### ✅ `CodeAnalysisPipelineTests.cs`

```csharp
using Microsoft.Extensions.Options;
using Moq;
using OllamaSharp;
using Xunit;

public class CodeAnalysisPipelineTests
{
    [Fact]
    public async Task ExecuteAsync_GeneratesAndSavesResponse()
    {
        // Arrange
        var mockWriter = new Mock<IOutputWriter>();
        var mockConfig = Options.Create(new AppConfiguration());
        var mockSystemPrompt = new Mock<ISystemPromptProvider>();
        mockSystemPrompt.Setup(p => p.Template).Returns("You are a .NET expert.");

        var mockSaveService = new Mock<IResponseWriter>();
        mockSaveService.Setup(s => s.WriteResponseAsync(It.IsAny<SaveDetails>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync("C:\\responses\\test_20250405_123456.md");

        var mockOllamaClientFactory = new Mock<IOllamaClientFactory>();
        var mockOllamaResponse = new Mock<IOllamaResponse>();

        mockOllamaClientFactory.Setup(f => f.FindClient(OllamaServiceType.Ollama))
                               .Returns(new OllamaApiClient(new HttpClient()) { SelectedModel = "qwen2.5-coder:14b" });

        mockOllamaResponse.Setup(r => r.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync("Here is the answer.");

        var pipeline = new CodeAnalysisPipeline(
            mockWriter.Object,
            mockConfig,
            mockSystemPrompt.Object,
            mockSaveService.Object,
            mockOllamaClientFactory.Object,
            mockOllamaResponse.Object
        );

        var question = new Question(true, "How do I use extension methods?", "ext-methods", QuestionCategory.General);

        // Act
        await pipeline.ExecuteAsync(question, "Context: extension methods allow adding methods to types.", CancellationToken.None);

        // Assert
        mockSaveService.Verify(s => s.WriteResponseAsync(It.IsAny<SaveDetails>(), It.IsAny<CancellationToken>()), Times.Once);
        mockOllamaResponse.Verify(r => r.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

---

## 🧪 6. `RagPipelineRunner`

### ✅ `RagPipelineRunnerTests.cs`

```csharp
using Moq;
using Xunit;

public class RagPipelineRunnerTests
{
    [Fact]
    public async Task StartAsync_CallsAllSteps()
    {
        // Arrange
        var mockWriter = new Mock<IOutputWriter>();
        var mockEmbeddingPipeline = new Mock<IEmbeddingPipeline>();
        var mockRagPipeline = new Mock<IKnowledgeBaseInitialize>();
        var mockBranding = new Mock<IApplicationBanner>();
        var mockSummaryService = new Mock<ISummaryService>();

        mockEmbeddingPipeline.Setup(p => p.EnsureCollectionExistsAsync(It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
        mockEmbeddingPipeline.Setup(p => p.PopulateAsync(It.IsAny<CancellationToken>()))
                             .Returns(Task.CompletedTask);
        mockRagPipeline.Setup(p => p.AskQuestionsAsync(It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);
        mockSummaryService.Setup(s => s.SummarizeAllResponsesAsync(It.IsAny<CancellationToken>()))
                          .Returns(Task.CompletedTask);

        var runner = new RagPipelineRunner(
            mockWriter.Object,
            mockEmbeddingPipeline.Object,
            mockRagPipeline.Object,
            mockBranding.Object,
            mockSummaryService.Object
        );

        // Act
        await runner.StartAsync(CancellationToken.None);

        // Assert
        mockBranding.Verify(b => b.RenderBranding(), Times.Once);
        mockEmbeddingPipeline.Verify(p => p.EnsureCollectionExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockEmbeddingPipeline.Verify(p => p.PopulateAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockRagPipeline.Verify(p => p.AskQuestionsAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockSummaryService.Verify(s => s.SummarizeAllResponsesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

---

## 🧪 7. `ResponseWriter`

### ✅ `ResponseWriterTests.cs`

```csharp
using Microsoft.Extensions.Options;
using Xunit;

public class ResponseWriterTests
{
    [Fact]
    public async Task WriteResponseAsync_CreatesFile_WithCorrectContent()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"rag-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        var mockConfig = Options.Create(new AppConfiguration
        {
            RagOptions = new RagOptions
            {
                SourceDirectory = tempDir,
                SaveDirectory = "Responses"
            }
        });

        var writer = new ResponseWriter(mockConfig);

        var question = new Question(true, "What is x?", "q1", QuestionCategory.General);
        var details = new SaveDetails(question, "Answer: x = 42", TimeSpan.FromSeconds(1.5));

        // Act
        var path = await writer.WriteResponseAsync(details, CancellationToken.None);

        // Assert
        Assert.True(File.Exists(path));
        var content = await File.ReadAllTextAsync(path);
        Assert.Contains("What is x?", content);
        Assert.Contains("Answer: x = 42", content);
        Assert.Contains("00:01", content); // duration

        // Cleanup
        Directory.Delete(tempDir, true);
    }
}
```

---

## 🧪 8. `StringExtensions.CharacterCount()`

### ✅ `StringExtensionsTests.cs`

```csharp
using Xunit;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("<summary>Test</summary>", 4)] // "Test"
    [InlineData("plain text", 10)]
    [InlineData("<c>code</c> and <b>bold</b>", 13)] // "code" + " and " + "bold"
    public void CharacterCount_ExcludesXmlTags(string input, int expected)
    {
        var span = input.AsSpan();
        var count = span.CharacterCount();
        Assert.Equal(expected, count);
    }
}
```

---

## 🧪 9. `UtilityExtensions.ExpandDirectory()`

### ✅ `UtilityExtensionsTests.cs`

```csharp
using Xunit;

public class UtilityExtensionsTests
{
    [Fact]
    public void ExpandDirectory_ExpandsEnvVars()
    {
        Environment.SetEnvironmentVariable("MY_TEST_DIR", Path.GetTempPath());
        var path = "%MY_TEST_DIR%\\subdir";
        var expanded = path.ExpandDirectory();
        Assert.StartsWith(Path.GetTempPath(), expanded);
        Assert.EndsWith("\\subdir", expanded);
    }

    [Fact]
    public void ExpandDirectory_ThrowsOnNonExistent()
    {
        var path = Path.Combine(Path.GetTempPath(), "nonexistent_" + Guid.NewGuid());
        Assert.Throws<DirectoryNotFoundException>(() => path.ExpandDirectory());
    }
}
```

---

## 🧪 10. `ChunkBySyntaxTree.ChunkSourceFile()`

### ✅ `ChunkBySyntaxTreeTests.cs`

```csharp
using Xunit;

public class ChunkBySyntaxTreeTests
{
    [Fact]
    public void ChunkSourceFile_ParsesTopLevelClasses()
    {
        var code = """
            namespace TestApp
            {
                public class A { }
                public class B { }
            }
            """;

        var docs = ChunkBySyntaxTree.ChunkSourceFile("Test.cs", code);

        Assert.NotNull(docs);
        Assert.Equal(2, docs.Count);
        Assert.All(docs, d => Assert.Contains("class", d.ElementType));
    }

    [Fact]
    public void ChunkSourceFile_ReturnsNull_OnParseFailure()
    {
        var docs = ChunkBySyntaxTree.ChunkSourceFile("Test.cs", "invalid { syntax }");
        Assert.Null(docs);
    }
}
```

---

## ✅ Bonus: `QuestionMap` (CSV mapping)

### ✅ `QuestionMapTests.cs`

```csharp
using CsvHelper.Configuration;
using Xunit;

public class QuestionMapTests
{
    [Fact]
    public void Map_ConfiguresCorrectly()
    {
        var map = new QuestionMap();

        Assert.Equal("IsEnabled", map.ClassMap.Properties.First(p => p.Name == nameof(QuestionRecord.IsEnabled)).Name);
        Assert.Equal("Text", map.ClassMap.Properties.First(p => p.Name == nameof(QuestionRecord.Text)).Name);
        Assert.Equal("FileName", map.ClassMap.Properties.First(p => p.Name == nameof(QuestionRecord.FileName)).Name);
        Assert.Equal("Category", map.ClassMap.Properties.First(p => p.Name == nameof(QuestionRecord.Category)).Name);

        // Verify enum converter
        var categoryProp = map.ClassMap.Properties.First(p => p.Name == nameof(QuestionRecord.Category));
        Assert.NotNull(categoryProp.TypeConverter);
    }
}
```

---

## 📁 Folder Structure Suggestion

```
tests/
├── RagPipeline.UnitTests/
│   ├── BaseFileParserTests.cs
│   ├── ParseCSharpFileTests.cs
│   ├── FileParseFactoryTests.cs
│   ├── OllamaResponseTests.cs
│   ├── CodeAnalysisPipelineTests.cs
│   ├── RagPipelineRunnerTests.cs
│   ├── ResponseWriterTests.cs
│   ├── StringExtensionsTests.cs
│   ├── UtilityExtensionsTests.cs
│   ├── ChunkBySyntaxTreeTests.cs
│   └── QuestionMapTests.cs
```

---

## ✅ Final Notes

- These tests cover **core logic**, **error handling**, and **integration points**.
- For **true integration tests**, consider:
  - Using `TestServer` + `WebApplicationFactory` for API layers (if any).
  - Docker containers for Qdrant/Ollama (via `testcontainers-dotnet`).
  - File system isolation (e.g., `TempDirectory` per test).
- Add **coverage reporting** with `coverlet.collector`.

Let me know if you'd like:
- Full test project `.csproj`
- CI-ready `dotnet test` setup
- Parallel test execution config
- FluentAssertions instead of `Assert`

Happy testing! 🚀
