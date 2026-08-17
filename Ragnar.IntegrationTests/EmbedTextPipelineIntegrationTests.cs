//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;

//using Ragnar.Core.Interface;
//using Ragnar.Core.Options;
//using Ragnar.Embedding.Factory;
//using Ragnar.Embedding.UnitOfWork;

//using Testcontainers.Qdrant;

//namespace Ragnar.IntegrationTests;

///// <summary>
///// Integration tests for the file embedding pipeline.
///// Validates directory traversal, syntax parsing, and batch upsert to vector store.
///// </summary>
//public class EmbedTextPipelineIntegrationTests
//    : IAsyncLifetime
//{
//    private readonly QdrantContainer _qdrant = new QdrantBuilder().Build();
//    private ServiceProvider? _services;
//    private string _testDir = "";

//    public async Task InitializeAsync()
//    {
//        await _qdrant.StartAsync();
//        _testDir = Path.Combine(Path.GetTempPath(), $"RagnarEmbedTest_{Guid.NewGuid()}");
//        Directory.CreateDirectory(_testDir);

//        var host = Host.CreateDefaultBuilder()
//            .ConfigureServices((ctx, services) =>
//            {
//                services.AddSingleton<IVectorStoreRepository>(sp => new QdrantVectorStoreRepository(_qdrant.Uri));
//                services.AddSingleton<IFileParseFactory, FileParseFactory>();
//                services.AddSingleton<IFileValidator, FileValidator>();
//                services.AddOptions<ApplicationConfiguration>()
//                    .Bind(ctx.Configuration.GetSection("ApplicationOptions"))
//                    .ValidateDataAnnotations()
//                    .ValidateOnStart();
//            })
//            .Build();

//        _services = host.Services;
//    }

//    public async Task DisposeAsync()
//    {
//        await _qdrant.StopAsync();
//        if(Directory.Exists(_testDir))
//            Directory.Delete(_testDir, true);
//        _services?.Dispose();
//    }

//    [Fact]
//    public async Task RunAsync_ShouldProcessFilesAndUpsertEmbeddings()
//    {
//        // Arrange
//        var pipeline = _services!.GetRequiredService<IEmbedTextPipeline>();
//        var testFile = Path.Combine(_testDir, "IntegrationTest.cs");
//        await File.WriteAllTextAsync(testFile, """
//                public class IntegrationTest 
//                {
//                    /// <summary>Validates pipeline behavior.</summary>
//                    public void Execute() { }
//                }
//                """);

//        // Act
//        await pipeline.RunAsync(CancellationToken.None);

//        // Assert
//        // Verify no exceptions and that the vector store received documents.
//    }

//    [Fact]
//    public async Task RunAsync_ShouldSkipInvalidExtensions()
//    {
//        // Arrange
//        var pipeline = _services!.GetRequiredService<IEmbedTextPipeline>();
//        var invalidFile = Path.Combine(_testDir, "data.json");
//        await File.WriteAllTextAsync(invalidFile, "{}");

//        // Act
//        await pipeline.RunAsync(CancellationToken.None);

//        // Assert
//        // Pipeline should gracefully ignore non-code files based on IFileValidator rules.
//    }
//}
