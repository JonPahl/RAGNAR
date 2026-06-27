//using Ragnar.Core.Model;
//using Ragnar.Core.Options;
//using Ragnar.Models;
//using Ragnar.Output;
//using Ragnar.Plugins;

//namespace RAGNAR.UnitTests;

//public sealed class ResponseWriterTests
//{
//    [Fact]
//    public async Task WriteResponseAsync_CreatesFile_WithCorrectPath()
//    {
//        // Arrange
//        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
//        Directory.CreateDirectory(tempDir);
//        try
//        {
//            var config = Options.CreateAsync(new ApplicationConfiguration
//            {
//                FileLoadOptions = new FileLoadOptions()
//                {

//                },
//                OllamaOptions = new OllamaOptions()
//                { Host = "", LlmModel = "", Port = 1, Timeout = TimeSpan.FromMinutes(1) },
//                EmbeddingOptions = new EmbeddingOptions()
//                { Dimension = 1, EmbeddingModel = "", Host = "", Port = 1, Timeout = TimeSpan.FromMinutes(1), },
//                ApplicationOptions = new()
//                {
//                    VectorStoreName = "",
//                    SourceDirectory = tempDir
//                }
//            });

//            var writer = new ResponseWriter(config);

//            var question = Question.IsActive("Test?", "test", QuestionCategory.Refactor);
//            var details = new SaveDetails(question, "Response text", "01:23");

//            // Act
//            var path = await writer.WriteResponseAsync(details, CancellationToken.None);

//            // Assert
//            Assert.StartsWith(tempDir, path);
//            Assert.Contains("Response", path);
//            Assert.Contains("Refactor", path);
//            Assert.EndsWith(".md", path);
//            Assert.True(File.Exists(path));
//        }
//        finally
//        {
//            Directory.Delete(tempDir, true);
//        }
//    }

//    [Fact]
//    public async Task WriteResponseAsync_UsesCorrectFormat()
//    {
//        // Arrange
//        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
//        Directory.CreateDirectory(tempDir);
//        try
//        {
//            var config = Options.CreateAsync(new ApplicationConfiguration
//            {
//                ApplicationOptions = new() { SourceDirectory = tempDir }
//            });

//            var writer = new ResponseWriter(config);

//            var question = Question.IsActive("Test?", "test", QuestionCategory.Refactor);
//            var details = new SaveDetails(question, "Response text", "01:23");

//            // Act
//            await writer.WriteResponseAsync(details, CancellationToken.None);

//            // Assert
//            var filePath = Directory.GetFiles(tempDir, "*.md", SearchOption.AllDirectories).Single();
//            var content = await File.ReadAllTextAsync(filePath);

//            Assert.Contains("## Question:", content);
//            Assert.Contains("> Test?", content);
//            Assert.Contains("## Response:", content);
//            Assert.Contains("Response text", content);
//        }
//        finally
//        {
//            Directory.Delete(tempDir, true);
//        }
//    }
//}
