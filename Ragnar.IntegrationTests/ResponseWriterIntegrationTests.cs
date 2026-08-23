namespace Ragnar.IntegrationTests;

/// <summary>
/// Integration tests for response persistence.
/// Validates file system operations and markdown formatting without external LLM dependencies.
/// </summary>
public class ResponseWriterIntegrationTests
{
    private string TempDir = "";
    private ServiceProvider? Services;

    public void Initialize()
    {
        TempDir = Path.Join(Path.GetTempPath(), $"RagnarResponseTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(TempDir);

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

        Services = (ServiceProvider?)host.Services;
    }

    public void Dispose()
    {
        if (Directory.Exists(TempDir))
            Directory.Delete(TempDir, true);
        Services?.Dispose();
    }
}
