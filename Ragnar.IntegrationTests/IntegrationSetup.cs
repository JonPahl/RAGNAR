namespace Ragnar.IntegrationTests;

/// <summary>Base class for integration test setup with pre - configured options.</summary>
public abstract class IntegrationSetup
{
    public readonly AppConfiguration options = new()
    {
        RagOptions = new()
        {
            SourceDirectory = "",
            VectorStoreName = "",
            SaveDirectory = ""
        },
        EmbeddingOptions = new EmbeddingOptions()
        {
            Dimension = 0,
            Host = "localhost",
            EmbeddingModel = "",
            Port = 0,
            Timeout = TimeSpan.FromMinutes(5)
        },
        FileLoadOptions = new()
        {

        },
        OllamaOptions = new()
        {
            Port = 0,
            Host = "localhost",
            CodeModel = "",
            Timeout = TimeSpan.FromMinutes(5)
        }
    };
}
