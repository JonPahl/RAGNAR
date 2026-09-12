//using Ragnar.Builder;
//using Ragnar.Embedding.Factory;

//namespace Ragnar.Tests;

//// ───────────────────────────────────────────────────────────────
////  7.  EmbedTextPipeline
//// ───────────────────────────────────────────────────────────────
//public class EmbedTextPipelineTests
//{
//    private static (EmbedTextPipeline pipeline, Mock<IOptions<RagnarConfig>> opts,
//                   Mock<IVectorStoreRepository> repo, Mock<Serilog.ILogger> logger,
//                   Mock<IFileParseFactory> parseFactory) CreatePipeline(
//        string? sourceDir = @"C:\Test\Source",
//        bool dirExists = true)
//    {
//        var config = new RagnarConfig
//        {
//            ApplicationOptions = new ApplicationOptions { SourceDirectory = sourceDir },
//            FileLoadOptions = new FileLoadOptions()
//        };
//        var mockOpts = new Mock<IOptions<RagnarConfig>>();
//        mockOpts.Setup(o => o.Value).Returns(config);

//        var mockRepo = new Mock<IVectorStoreRepository>();
//        var mockLogger = new Mock<Serilog.ILogger>();
//        var mockParse = new Mock<IFileParseFactory>();

//        var pipeline = new EmbedTextPipeline(mockOpts.Object, mockRepo.Object, mockLogger.Object, mockParse.Object);
//        return (pipeline, mockOpts, mockRepo, mockLogger, mockParse);
//    }

//    [Fact]
//    public async Task RunAsyncLogsWarningAndReturnsWhenSourceDirNotFound()
//    {
//        // Use a path that definitely doesn't exist
//        var (pipeline, _, mockLogger, _, _) = CreatePipeline(sourceDir: @"C:\Does\Not\Exist\XYZ12345");

//        await pipeline.RunAsync(CancellationToken.None);

//        mockLogger.Verify(l => l.Warning(It.Is<string>(m => m.Contains("Source directory not found")), It.IsAny<object[]>()), Times.Once);
//    }

//    [Fact]
//    public void EmbedTextPipelineImplementsIEmbedTextPipeline()
//    {
//        var (pipeline, _, _, _, _) = CreatePipeline();
//        Assert.IsAssignableFrom<IEmbedTextPipeline>(pipeline);
//    }
//}
