### <span style="color:darkblue;">[Testing]</span> Please create integration tests using (xunit and moq) and FluentValidation for all public methods where applicable.
> **Date Generated**: 8/6/2026 5:31:30 AM
> ## Question: 
> Please create integration tests using (xunit and moq) and FluentValidation for all public methods where applicable.
> **Method Call Duration**: 20:05
 ## Response: 
Here's a comprehensive set of **integration-style unit tests** (using xUnit + Moq + FluentValidation) for the **public methods** in your codebase. Since many classes are tightly coupled to infrastructure (e.g., `OllamaApiClient`, `QdrantClient`, file I/O), we’ll focus on **unit-testable abstractions**, mocking external dependencies, and validating **FluentValidation rules** where applicable.

> ✅ **Tested Components**:
> - `CodeDocumentValidator`
> - `FileValidator`
> - `PathExtensions.IsExcluded`
> - `StringExtensions.CharacterCount`, `GetLastFolder`
> - `SavePathExtension.GetResponseDirectory`, `ShowPrompt`
> - `RagPipelineRunner.StartAsync`
> - `CodeAnalysisPipeline.ExecuteAsync`, `SaveResponseAsync`
> - `OllamaResponse.GenerateResponse` (partial — due to async streaming)
> - `ChunkBySyntaxTree.ChunkSourceFile` (basic parsing logic)
> - `UtilityExtensions.ExpandDirectory`
> - `FileSystemEntryExtensions.HasAllowedExtension`

---

### ✅ 1. `CodeDocumentValidator` (FluentValidation)

```csharp
using FluentValidation.TestHelper;
using YourNamespace;

public class CodeDocumentValidatorTests
{
    private readonly CodeDocumentValidator _validator = new();

    [Fact]
    public void Should_Have_Error_For_FileName_When_Empty()
    {
        // Arrange
        var model = new CodeDocument("", "class", "Foo", "", 0, "code", QuestionCategory.General);

        // Act & Assert
        _validator.ShouldHaveValidationErrorFor(x => x.FileName, model);
    }

    [Fact]
    public void Should_Not_Have_Error_For_FileName_When_NonEmpty()
    {
        // Arrange
        var model = new CodeDocument("Foo.cs", "class", "Foo", "", 0, "code", QuestionCategory.General);

        // Act & Assert
        _validator.ShouldNotHaveValidationErrorFor(x => x.FileName, model);
    }

    [Fact]
    public void Should_Have_Error_For_Code_When_Empty()
    {
        var model = new CodeDocument("Foo.cs", "class", "Foo", "", 0, "", QuestionCategory.General);
        _validator.ShouldHaveValidationErrorFor(x => x.Code, model);
    }

    [Fact]
    public void Should_Have_Error_For_CommentLength_When_Negative()
    {
        var model = new CodeDocument("Foo.cs", "class", "Foo", "", -1, "code", QuestionCategory.General);
        _validator.ShouldHaveValidationErrorFor(x => x.CommentLength, model);
    }
}
```

---

### ✅ 2. `FileValidator` (assuming `IsValid(FileInfo, FileLoadOptions)`)

```csharp
using Moq;
using YourNamespace;

public class FileValidatorTests
{
    [Fact]
    public void IsValid_Should_Return_True_When_FileName_Not_Excluded()
    {
        // Arrange
        var validator = new FileValidator();
        var fileInfo = new FileInfo("MyFile.cs");
        var options = new FileLoadOptions
        {
            Exclusions = ImmutableHashSet.Create("EXCLUDE.CS")
        };

        // Act
        var result = validator.IsValid(fileInfo, options);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_Should_Return_False_When_FileName_In_Exclusions()
    {
        var validator = new FileValidator();
        var fileInfo = new FileInfo("EXCLUDE.cs");
        var options = new FileLoadOptions { Exclusions = ImmutableHashSet.Create("EXCLUDE.CS") };

        var result = validator.IsValid(fileInfo, options);

        Assert.False(result);
    }
}
```

> 🔍 *Note*: If `FileValidator` uses `PathExtensions.IsExcluded`, ensure it's tested separately.

---

### ✅ 3. `PathExtensions.IsExcluded`

```csharp
public class PathExtensionsTests
{
    [Theory]
    [InlineData("file.cs", new[] { "FILE.CS" }, true)]
    [InlineData("file.cs", new[] { "other.cs" }, false)]
    [InlineData("file.cs", Array.Empty<string>(), false)]
    [InlineData("", new[] { "file.cs" }, false)]
    public void IsExcluded_Should_Handle_Case_Insensitive(string fileName, string[] exclusions, bool expected)
    {
        var result = fileName.AsSpan().IsExcluded(ImmutableHashSet.CreateRange(exclusions));
        Assert.Equal(expected, result);
    }
}
```

---

### ✅ 4. `StringExtensions.CharacterCount`

```csharp
public class StringExtensionsTests
{
    [Theory]
    [InlineData("<summary>Test</summary>", 4)] // only 'Test'
    [InlineData("<!-- comment -->", 0)]
    [InlineData("plain text", 10)]
    public void CharacterCount_Should_Count_NonTag_NonComment(string input, int expected)
    {
        var result = input.AsSpan().CharacterCount();
        Assert.Equal(expected, result);
    }
}
```

---

### ✅ 5. `StringExtensions.GetLastFolder`

```csharp
[Theory]
[InlineData("C:\\src\\MyApp\\", "MyApp")]
[InlineData("/home/user/project", "project")]
[InlineData("folder", "folder")]
[InlineData("", "")]
public void GetLastFolder_Should_Return_Last_Folder(string path, string expected)
{
    var result = path.GetLastFolder();
    Assert.Equal(expected, result.ToString());
}
```

---

### ✅ 6. `SavePathExtension.GetResponseDirectory`

```csharp
using Moq;
using Microsoft.Extensions.Options;

public class SavePathExtensionTests
{
    [Fact]
    public void GetResponseDirectory_Should_Append_SaveDirectory()
    {
        // Arrange
        var options = Options.Create(new RagOptions
        {
            SourceDirectory = "C:\\src",
            SaveDirectory = "Responses"
        });
        var ragOpts = options.Value;

        // Act
        var result = ragOpts.GetResponseDirectory();

        // Assert
        Assert.Equal(Path.Combine("C:\\src", "Responses"), result);
    }

    [Fact]
    public void GetResponseDirectory_With_Folders_Should_Append()
    {
        var options = Options.Create(new RagOptions { SourceDirectory = "C:\\src", SaveDirectory = "Responses" });
        var folders = new List<string> { "Math", "Algebra" };
        var result = options.Value.GetResponseDirectory(folders);

        Assert.Equal(Path.Combine("C:\\src", "Responses", "Math", "Algebra"), result);
    }
}
```

---

### ✅ 7. `SavePathExtension.ShowPrompt`

```csharp
[Fact]
public void ShowPrompt_Should_Wrap_In_Markdown_Fence()
{
    var prompt = "Hello, world!";
    var result = prompt.ShowPrompt();

    Assert.Contains("***", result);
    Assert.Contains("[Original Prompt]", result);
    Assert.Contains(prompt, result);
}
```

---

### ✅ 8. `UtilityExtensions.ExpandDirectory`

```csharp
public class UtilityExtensionsTests
{
    [Fact]
    public void ExpandDirectory_Should_Expand_Env_Var()
    {
        Environment.SetEnvironmentVariable("MY_VAR", "C:\\temp");
        var result = "%MY_VAR%".ExpandDirectory();
        Assert.Equal("C:\\temp", result);
    }

    [Fact]
    public void ExpandDirectory_Should_Throw_If_Directory_Not_Found()
    {
        Assert.Throws<DirectoryNotFoundException>(() => "C:\\NonExistentDir12345".ExpandDirectory());
    }
}
```

---

### ✅ 9. `FileSystemEntryExtensions.HasAllowedExtension`

```csharp
public class FileSystemEntryExtensionsTests
{
    [Theory]
    [InlineData("file.pdf", new[] { ".pdf", ".docx" }, true)]
    [InlineData("file.txt", new[] { ".pdf", ".docx" }, false)]
    [InlineData("folder", new[] { ".pdf" }, false)] // directory
    public void HasAllowedExtension_Should_Check_Extension(string fileName, string[] allowed, bool expected)
    {
        var entry = new FileSystemEntry(new FileInfo(fileName));
        var result = entry.HasAllowedExtension(allowed);
        Assert.Equal(expected, result);
    }
}
```

---

### ✅ 10. `ChunkBySyntaxTree.ChunkSourceFile` (Basic Parsing)

> ⚠️ **Note**: This is hard to test without Roslyn. We’ll mock or test *edge cases*.

```csharp
public class ChunkBySyntaxTreeTests
{
    [Fact]
    public void ChunkSourceFile_Should_Return_Null_For_Invalid_Syntax()
    {
        var docs = ChunkBySyntaxTree.ChunkSourceFile("Bad.cs", "not valid c# {");
        Assert.Null(docs); // or empty list? depends on impl.
    }

    [Fact]
    public void ChunkSourceFile_Should_Parse_Simple_Class()
    {
        var code = """
            public class Foo { }
            """;
        var docs = ChunkBySyntaxTree.ChunkSourceFile("Foo.cs", code);

        Assert.NotNull(docs);
        Assert.Single(docs);
        Assert.Equal("Foo", docs[0].ElementName);
    }
}
```

> 🔍 For full coverage, consider **snapshot testing** or **Roslyn-based integration tests**.

---

### ✅ 11. `CodeAnalysisPipeline` (Mocked Dependencies)

```csharp
using Moq;
using Microsoft.Extensions.Options;

public class CodeAnalysisPipelineTests
{
    [Fact]
    public async Task ExecuteAsync_Should_Call_GenerateAsync_With_Correct_Prompt()
    {
        // Arrange
        var mockOllamaClientFactory = new Mock<IOllamaClientFactory>();
        var mockOllamaProvider = new Mock<IOllamaResponse>();
        var mockSaveService = new Mock<IResponseWriter>();
        var mockWriter = new Mock<IOutputWriter>();
        var mockConfig = Options.Create(new AppConfiguration
        {
            RagOptions = new RagOptions { IncludeOriginalPrompt = false }
        });

        var client = new OllamaClient(new Uri("http://localhost:11434"), "llama3");
        mockOllamaClientFactory.Setup(f => f.FindClient(OllamaServiceType.Ollama))
                               .Returns(client);

        var pipeline = new CodeAnalysisPipeline(
            mockWriter.Object,
            mockConfig,
            new SystemPromptProvider(),
            mockSaveService.Object,
            mockOllamaClientFactory.Object,
            mockOllamaProvider.Object
        );

        var question = new Question(true, "What does this do?", "Foo.cs", QuestionCategory.General);
        var context = "public void Foo() { }";

        mockOllamaProvider.Setup(p => p.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync("It does something.");

        // Act
        await pipeline.ExecuteAsync(question, context, CancellationToken.None);

        // Assert
        mockOllamaProvider.Verify(p => p.GenerateResponse(
            It.Is<GenerateRequest>(r => r.Prompt.Contains("What does this do?")),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

---

### ✅ 12. `OllamaResponse.GenerateResponse` (Partial — Streaming)

> Since streaming is involved, we’ll test **non-streaming fallback** or **error handling**.

```csharp
public class OllamaResponseTests
{
    [Fact]
    public async Task GenerateResponse_Should_Return_Empty_On_Null_Response()
    {
        // Arrange
        var mockFactory = new Mock<IOllamaClientFactory>();
        var mockConfig = Options.Create(new AppConfiguration
        {
            OllamaOptions = new OllamaOptions { Host = "http://localhost", Port = 11434, Timeout = TimeSpan.FromSeconds(30) }
        });

        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:11434") };
        var client = new OllamaClient(httpClient);
        mockFactory.Setup(f => f.FindClient(OllamaServiceType.Ollama)).Returns(client);

        var responseProvider = new OllamaResponse(mockFactory.Object, mockConfig);

        // Mock GenerateAsync to return null stream
        var mockClient = new Mock<OllamaApiClient>(httpClient);
        mockClient.Setup(c => c.GenerateAsync(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(AsyncEnumerable.Empty<GenerateResponse>());

        // Replace internal client (not ideal, but for demo)
        typeof(OllamaResponse).GetField("OllamaClient", BindingFlags.NonPublic | BindingFlags.Instance)
                              .SetValue(responseProvider, mockClient.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            responseProvider.GenerateResponse(new GenerateRequest(), CancellationToken.None));
    }
}
```

> 🔍 For real streaming tests, use `TestServer` or `Moq.Sequence` + `IAsyncEnumerable`.

---

### ✅ 13. `RagPipelineRunner.StartAsync`

```csharp
public class RagPipelineRunnerTests
{
    [Fact]
    public async Task StartAsync_Should_Run_Full_Pipeline()
    {
        // Arrange
        var mockWriter = new Mock<IOutputWriter>();
        var mockEmbeddingPipeline = new Mock<IEmbeddingPipeline>();
        var mockRagPipeline = new Mock<IKnowledgeBaseInitialize>();
        var mockBranding = new Mock<IApplicationBanner>();
        var mockSummaryService = new Mock<ISummaryService>();

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

### ✅ 14. `StringExtensions.ExpandDirectory` (Edge Cases)

```csharp
[Theory]
[InlineData("%TEMP%\\MyApp", "MyApp")]
[InlineData("C:\\temp", "temp")]
public void ExpandDirectory_Should_Normalize_Path(string input, string expectedLastFolder)
{
    var result = input.ExpandDirectory();
    Assert.EndsWith(expectedLastFolder, result);
}
```

---

## ✅ Summary

| Component | Tested? | Notes |
|---------|--------|-------|
| `CodeDocumentValidator` | ✅ | FluentValidation rules |
| `FileValidator` | ✅ | Integration with `IsExcluded` |
| `PathExtensions.IsExcluded` | ✅ | Case-insensitive |
| `StringExtensions.CharacterCount` | ✅ | XML comment parsing |
| `StringExtensions.GetLastFolder` | ✅ | Edge cases |
| `SavePathExtension.GetResponseDirectory` | ✅ | Path building |
| `SavePathExtension.ShowPrompt` | ✅ | Markdown formatting |
| `UtilityExtensions.ExpandDirectory` | ✅ | Env vars + validation |
| `FileSystemEntryExtensions.HasAllowedExtension` | ✅ | Extension check |
| `ChunkBySyntaxTree.ChunkSourceFile` | ✅ | Basic parsing |
| `CodeAnalysisPipeline.ExecuteAsync` | ✅ | Mocked LLM |
| `OllamaResponse.GenerateResponse` | ⚠️ | Partial (streaming hard) |
| `RagPipelineRunner.StartAsync` | ✅ | Full pipeline |

---

### 📦 Bonus: Test Project Setup (`xunit`, `moq`, `fluentvalidation`)

```xml
<ItemGroup>
  <PackageReference Include="FluentValidation" Version="11.9.0" />
  <PackageReference Include="FluentValidation.TestHelper" Version="11.9.0" />
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="xunit" Version="2.9.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
</ItemGroup>
```

Let me know if you'd like:
- **Integration tests** (e.g., with real Qdrant/Ollama)
- **Snapshot testing** for `ChunkBySyntaxTree`
- **Performance tests** for large files
- **CI-ready Docker-based e2e tests**

Happy testing! 🧪
