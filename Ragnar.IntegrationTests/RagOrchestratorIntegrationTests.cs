//namespace Ragnar.IntegrationTests;

///// <summary>
///// Integration tests for the RAG pipeline orchestrator.
///// Validates end-to-end flow: question processing → Ollama generation → response saving.
///// </summary>
//public class RagOrchestratorIntegrationTests
//    : IAsyncLifetime
//{
//    private readonly OllamaContainer _ollama = new OllamaBuilder().Build();
//    private readonly QdrantContainer _qdrant = new QdrantBuilder().Build();
//    private ServiceProvider? _services;

//    public async Task InitializeAsync()
//    {
//        await _ollama.StartAsync();
//        await _qdrant.StartAsync();

//        var host = Host.CreateDefaultBuilder()
//            .ConfigureServices((ctx, services) =>
//            {
//                // Wire real implementations to test containers
//                services.AddSingleton<IOllamaClientProvider>(sp => new OllamaClientProvider(new Uri(_ollama.GetEndpoint())));
//                services.AddSingleton<IOllamaResponse, OllamaResponse>();
//                services.AddSingleton<IVectorStoreRepository>(sp => new QdrantVectorStoreRepository(_qdrant.Uri));

//                // Register keyed service for system prompt as expected by RagOrchestrator
//                services.AddKeyedSingleton<ISystemPromptProvider, SystemPromptProvider>("Common");

//                services.AddSingleton<IResponseWriter, ResponseWriter>();
//                services.AddSingleton<IOutputWriter, AnsiConsoleOutputWriter>();
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
//        await _ollama.StopAsync();
//        await _qdrant.StopAsync();
//        _services?.Dispose();
//    }

//    [Fact]
//    public async Task RunAsync_ShouldProcessQuestionAndPersistResponse()
//    {
//        // Arrange
//        var orchestrator = _services!.GetRequiredService<IRagOrchestrator>();
//        var question = new Question(false, "Explain the purpose of ChunkBySyntaxTree.", "test_q_01", QuestionCategory.Refactor);
//        var contextText = "ChunkBySyntaxTree parses C# syntax trees into CodeDocument chunks for embedding.";

//        // Act
//        await orchestrator.RunAsync(question, contextText, CancellationToken.None);

//        // Assert
//        // In a real integration suite, verify vector store contains the query embedding 
//        // and response file exists in the configured output directory.
//    }

//    [Fact]
//    public async Task RunAsync_ShouldHandleCancellationGracefully()
//    {
//        // Arrange
//        var orchestrator = _services!.GetRequiredService<IRagOrchestrator>();
//        var question = new Question(false, "Long running query", "test_q_02", QuestionCategory.Performance);
//        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

//        // Act & Assert
//        await Assert.ThrowsAnyAsync<TaskCanceledException>(() =>
//            orchestrator.RunAsync(question, "Context", cts.Token));
//    }
//}
