namespace Ragnar.IntegrationTests;

/// <summary>
/// Integration tests for response persistence.
/// Validates file system operations and markdown formatting without external LLM dependencies.
/// </summary>
public class ResponseWriterIntegrationTests
{
    private string _tempDir = "";
    private ServiceProvider? _services;

    public void Initialize()
    {
        _tempDir = Path.Join(Path.GetTempPath(), $"RagnarResponseTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((ctx, services) =>
            {
                services.AddOptions<RagnarConfig>()
                    .Bind(ctx.Configuration.GetSection("ApplicationOptions"))
                    .ValidateDataAnnotations()
                    .ValidateOnStart();
                services.AddSingleton<IResponseWriter, ResponseWriter>();
            })
            .Build();

        _services = (ServiceProvider?)host.Services;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
        _services?.Dispose();
    }
}
