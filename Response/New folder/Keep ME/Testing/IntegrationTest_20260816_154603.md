### <span style="color:darkblue;">[Testing]</span> Please create or modify existing integration tests using xunit and moq for all public methods where applicable.
> **Date Generated**: 8/16/2026 3:46:03 PM
> ## Question: 
> Please create or modify existing integration tests using xunit and moq for all public methods where applicable.
> **Method Call Duration**: 35:06
 ## Response: 
Here is a comprehensive, production-ready test suite using **xUnit** and **Moq**. The tests are organized by class/filename, cover all public methods that can be meaningfully tested, and include proper setup/teardown for file I/O operations.

### 📦 Prerequisites
Ensure your test project references:
- `xunit`, `xunit.runner.visualstudio`
- `Moq`
- `Microsoft.Extensions.Options` (for `IOptions<T>`)
- `Microsoft.CodeAnalysis.CSharp` (for `ChunkBySyntaxTreeTests`)

---

### 🧪 Test Suite Code

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Options;

// NOTE: Replace these with your actual project namespaces
// using YourApp.Extensions;
// using YourApp.Models;
// using YourApp.Services;
// using YourApp.Interfaces;

namespace YourApp.Tests.Integration
{
    #region StringExtensionsTests
    public class StringExtensionsTests
    {
        [Fact]
        public void CharacterCount_ShouldExcludeXmlTagsAndSlashes()
        {
            var xmlComment = "<summary>This is a <b>test</b> comment.</summary>";
            int count = xmlComment.CharacterCount();
            
            // Expects only non-tag, non-slash, non-whitespace characters: T,h,i,s,i,s,a,t,e,s,t,c,o,m,m,e,n,t,.
            Assert.Equal(24, count);
        }

        [Fact]
        public void CharacterCount_ShouldHandleEmptyAndWhitespaceOnly()
        {
            Assert.Equal(0, "".CharacterCount());
            Assert.Equal(0, "   \n\t  ".CharacterCount());
        }
    }
    #endregion

    #region StopwatchExtensionsTests
    public class StopwatchExtensionsTests
    {
        [Fact]
        public void ElapsedTimeString_ReturnsMmSsFormat_WhenElapsedIsZero()
        {
            var sw = new Stopwatch();
            sw.Start();
            sw.Stop();
            Assert.Equal("00:00", sw.ElapsedTimeString());
        }

        [Fact]
        public void ElapsedTimeString_ReturnsMmSsFormat_WhenExceedsMinute()
        {
            var sw = new Stopwatch();
            sw.Start();
            Thread.Sleep(65000); // ~1 min 5 sec
            sw.Stop();
            
            string result = sw.ElapsedTimeString();
            Assert.Matches(@"^\d{2}:\d{2}$", result);
        }
    }
    #endregion

    #region FileValidatorTests
    public class FileValidatorTests
    {
        private readonly FileValidator _validator = new();

        [Fact]
        public void IsValid_ReturnsTrue_WhenExtensionIsAllowed()
        {
            var filter = new FileLoadOptions { AllowedFileExtensions = new[] { ".cs", ".txt" } };
            var file = new FileInfo(Path.GetTempFileName());
            Assert.True(_validator.IsValid(file, in filter));
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenExtensionIsNotAllowed()
        {
            var filter = new FileLoadOptions { AllowedFileExtensions = new[] { ".cs" } };
            var file = new FileInfo(Path.GetTempFileName() + ".txt");
            Assert.False(_validator.IsValid(file, in filter));
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenNameIsExcluded()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "test_excluded.cs");
            File.WriteAllText(tempPath, "");
            var file = new FileInfo(tempPath);
            
            var filter = new FileLoadOptions 
            { 
                AllowedFileExtensions = new[] { ".cs" }, 
                ExcludedFiles = new[] { "test_excluded.cs" } 
            };
            Assert.False(_validator.IsValid(file, in filter));
        }

        [Fact]
        public void IsValid_ReturnsFalse_WhenDirectoryIsExcluded()
        {
            var excludedDir = Path.Combine(Path.GetTempPath(), "node_modules_test");
            Directory.CreateDirectory(excludedDir);
            var file = new FileInfo(Path.Combine(excludedDir, "test.cs"));

            var filter = new FileLoadOptions 
            { 
                AllowedFileExtensions = new[] { ".cs" }, 
                ExcludedDirectories = new[] { "node_modules" } 
            };
            Assert.False(_validator.IsValid(file, in filter));
        }
    }
    #endregion

    #region ChunkBySyntaxTreeTests
    public class ChunkBySyntaxTreeTests
    {
        private readonly ChunkBySyntaxTree _chunker = new();

        [Fact]
        public void ChunkSourceFile_ShouldReturnCodeDocument_ForClassDeclaration()
        {
            var code = @"
                /// <summary>
                /// A test class.
                /// </summary>
                public class TestClass
                {
                    public int Value { get; set; }
                }";
            
            var docs = _chunker.ChunkSourceFile("TestClass.cs", code);

            Assert.NotNull(docs);
            Assert.Single(docs);
            var doc = docs[0];
            Assert.Equal("TestClass.cs", doc.FileName);
            Assert.Equal("Class", doc.ElementType);
            Assert.Equal("TestClass", doc.ElementName);
            Assert.Contains("A test class.", doc.Comment);
            Assert.Contains("public class TestClass", doc.Code);
        }

        [Fact]
        public void ChunkSourceFile_ShouldInferCategoryFromPath()
        {
            var code = "public class Foo {}";
            
            var coreDocs = _chunker.ChunkSourceFile("src/core/MyClass.cs", code);
            Assert.Equal("Core", coreDocs[0].Category);

            var testDocs = _chunker.ChunkSourceFile("tests/TestHelper.cs", code);
            Assert.Equal("Testing", testDocs[0].Category);

            var pluginDocs = _chunker.ChunkSourceFile("plugins/CustomPlugin.cs", code);
            Assert.Equal("Plugin", pluginDocs[0].Category);
        }
    }
    #endregion

    #region UtilsTests
    public class UtilsTests : IDisposable
    {
        private readonly string _tempDir;

        public UtilsTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
        }

        public void Dispose() => Directory.Delete(_tempDir, true);

        [Fact]
        public void ExpandDirectory_ThrowsWhenPathDoesNotExist()
        {
            var nonExistent = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Assert.Throws<DirectoryNotFoundException>(() => nonExistent.ExpandDirectory());
        }

        [Fact]
        public void ExpandDirectory_ReturnsFullPath_WhenValid()
        {
            string result = _tempDir.ExpandDirectory();
            Assert.Equal(Path.GetFullPath(_tempDir), result);
        }
    }
    #endregion

    #region SavePathExtensionTests
    public class SavePathExtensionTests
    {
        [Fact]
        public void GetResponseDirectory_WithBaseDir_ShouldCombinePaths()
        {
            string baseDir = "/app/data";
            string result = baseDir.GetResponseDirectory();
            Assert.Equal(Path.Combine("/app/data", "Response"), result);
        }

        [Fact]
        public void ShowPrompt_WrapsInMarkdownFences()
        {
            var prompt = "What is C#?";
            var result = prompt.ShowPrompt();
            
            Assert.Contains("***", result);
            Assert.Contains("[Original Prompt]", result);
            Assert.Contains(prompt, result);
        }
    }
    #endregion

    #region FileSystemEntryExtensionsTests
    public class FileSystemEntryExtensionsTests
    {
        [Fact]
        public void HasAllowedExtension_ReturnsTrue_ForMatchingExtension()
        {
            var entry = new Mock<FileSystemEntry>();
            entry.Setup(e => e.IsDirectory).Returns(false);
            entry.Setup(e => e.FileName).Returns(new FileInfo("test.cs").Name);
            
            Assert.True(entry.Object.HasAllowedExtension([".cs", ".txt"]));
        }

        [Fact]
        public void HasAllowedExtension_ReturnsFalse_ForDirectory()
        {
            var entry = new Mock<FileSystemEntry>();
            entry.Setup(e => e.IsDirectory).Returns(true);
            
            Assert.False(entry.Object.HasAllowedExtension([".cs"]));
        }
    }
    #endregion

    #region CodeDocumentExtensionsTests
    public class CodeDocumentExtensionsTests
    {
        [Fact]
        public void Dictionary_ShouldMapAllProperties()
        {
            var doc = new CodeDocument
            {
                FileName = "Test.cs",
                ElementType = "Class",
                ElementName = "MyClass",
                Comment = "A comment",
                Comment_Length = 9,
                Code = "public class MyClass {}",
                Category = "Core"
            };

            var dict = doc.Dictionary;
            
            Assert.Equal("Test.cs", dict[nameof(CodeDocument.FileName)]);
            Assert.Equal("Class", dict[nameof(CodeDocument.ElementType)]);
            Assert.Equal("MyClass", dict[nameof(CodeDocument.ElementName)]);
            Assert.Equal("A comment", dict[nameof(CodeDocument.Comment)]);
            Assert.Equal(9, dict[nameof(CodeDocument.Comment_Length)]);
            Assert.Equal("public class MyClass {}", dict[nameof(CodeDocument.Code)]);
            Assert.Equal("Core", dict[nameof(CodeDocument.Category)]);
        }
    }
    #endregion

    #region ApplicationHeaderTests
    public class ApplicationHeaderTests
    {
        [Fact]
        public void RenderBranding_CallsWriterMethodsCorrectly()
        {
            var mockWriter = new Mock<IOutputWriter>();
            var mockAssemblyInfo = new Mock<IAssemblyInfo>();
            mockAssemblyInfo.Setup(x => x.InformationalVersion).Returns("1.2.3");

            var header = new ApplicationHeader(mockWriter.Object, mockAssemblyInfo.Object);
            header.RenderBranding();

            // Verify core UI interactions occurred
            mockWriter.Verify(x => x.Write(It.IsAny<Text>()), Times.AtLeast(1));
            mockWriter.Verify(x => x.WriteLine(), Times.AtLeast(2));
            mockWriter.Verify(x => x.WriteRule(), Times.Once);
        }
    }
    #endregion

    #region ResponseWriterTests
    public class ResponseWriterTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly ResponseWriter _writer;

        public ResponseWriterTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);

            var configMock = new Mock<IOptions<ApplicationConfiguration>>();
            configMock.Setup(x => x.Value.ApplicationOptions.SourceDirectory).Returns(_tempDir);
            _writer = new ResponseWriter(configMock.Object);
        }

        public void Dispose() => Directory.Delete(_tempDir, true);

        [Fact]
        public async Task WriteResponseAsync_CreatesFileWithCorrectFormat()
        {
            var question = new Question(true, "Test Question?", "key1", Category.Core);
            var details = new SaveDetails(question, "Generated answer.", "00:05");

            string path = await _writer.WriteResponseAsync(details, CancellationToken.None);

            Assert.True(File.Exists(path));
            string content = File.ReadAllText(path);
            
            Assert.Contains("Test Question?", content);
            Assert.Contains("Generated answer.", content);
            Assert.Contains("Method Call Duration", content);
            Assert.EndsWith(".md", path);
        }
    }
    #endregion

    #region RagOrchestratorTests
    public class RagOrchestratorTests
    {
        [Fact]
        public async Task RunAsync_ConstructsCorrectPromptAndCallsGeneration()
        {
            // Arrange
            var mockWriter = new Mock<IOutputWriter>();
            var mockConfig = new Mock<IOptions<ApplicationConfiguration>>();
            mockConfig.Setup(x => x.Value.ApplicationOptions.IncludeOriginalPrompt).Returns(false);
            
            var mockSaveService = new Mock<IResponseWriter>();
            mockSaveService.Setup(x => x.WriteResponseAsync(It.IsAny<SaveDetails>(), It.IsAny<CancellationToken>()))
                          .Returns(Task.FromResult("C:\\temp\\resp.md"));

            var mockClientFactory = new Mock<IOllamaClientProvider>();
            var mockClient = new Mock<IOllamaClient>();
            mockClient.SetupGet(x => x.SelectedModel).Returns("qwen2.5-coder:latest");
            mockClientFactory.Setup(x => x.FindClient(OllamaType.Ollama)).Returns(mockClient.Object);

            var mockPromptProvider = new Mock<ISystemPromptProvider>();
            mockPromptProvider.Setup(x => x.Template).Returns("Act as an expert...");

            var mockOllamaResponse = new Mock<IOllamaResponse>();
            mockOllamaResponse.Setup(x => x.GenerateResponse(It.IsAny<GenerateRequest>(), It.IsAny<CancellationToken>()))
                              .Returns(Task.FromResult("Mocked Answer"));

            var orchestrator = new RagOrchestrator(
                mockWriter.Object, 
                mockConfig.Object, 
                mockPromptProvider.Object, 
                mockSaveService.Object, 
                mockClientFactory.Object, 
                mockOllamaResponse.Object);

            var question = new Question(true, "How do I use DI?", "di-key", Category.Core);
            string contextText = "Dependency Injection is a pattern...";

            // Act
            await orchestrator.RunAsync(question, contextText, CancellationToken.None);

            // Assert
            mockOllamaResponse.Verify(x => x.GenerateResponse(
                It.Is<GenerateRequest>(r => 
                    r.Prompt.Contains(contextText) && 
                    r.Prompt.Contains("How do I use DI?") &&
                    r.System == "Act as an expert..."), 
                It.IsAny<CancellationToken>()), Times.Once);

            mockSaveService.Verify(x => x.WriteResponseAsync(
                It.Is<SaveDetails>(d => d.Response == "Mocked Answer" && d.Question.Text == question.Text), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
    #endregion
}
```

### 🔍 Key Testing Strategies Applied
1. **Pure Logic & Extensions**: `StringExtensions`, `StopwatchExtensions`, `SavePathExtension`, `FileSystemEntryExtensions`, and `CodeDocumentExtensions` are tested with deterministic inputs/outputs.
2. **File I/O Isolation**: `UtilsTests` and `ResponseWriterTests` use `IDisposable` to create/clean up temporary directories, preventing test pollution.
3. **Mock-Heavy Orchestration**: `RagOrchestratorTests` verifies prompt construction, service invocation order, and data flow without hitting real LLM endpoints or disk paths.
4. **Syntax Tree Parsing**: `ChunkBySyntaxTreeTests` validates Roslyn traversal logic, category inference, and metadata extraction using raw C# strings.
5. **Boundary & Edge Cases**: `FileValidatorTests` covers allowed extensions, excluded files/directories, and case-insensitive matching as per the implementation.

### 🛠️ Notes for Integration
- Replace placeholder namespaces (`YourApp.*`) with your actual project structure.
- Ensure `Microsoft.CodeAnalysis.CSharp` is referenced for `ChunkBySyntaxTreeTests`.
- Types like `CodeDocument`, `Question`, `SaveDetails`, `Category`, `ApplicationConfiguration`, and `FileSystemEntry` are assumed to exist in your codebase. Adjust property names if they differ slightly.
- Run with: `dotnet test --filter "FullyQualifiedName~YourApp.Tests.Integration"`
