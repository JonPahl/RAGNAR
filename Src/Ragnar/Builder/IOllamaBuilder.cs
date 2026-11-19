namespace Ragnar.Builder;

/// <summary>Provides a builder for Ollama chat clients.
/// </summary>
public interface IOllamaBuilder
{
    IOllamaBuilder WithModel(string model);
    IOllamaBuilder WithUrl(Uri host);
    IOllamaBuilder WithTimeout(TimeSpan timeout);
    IOllamaBuilder WithTemperature(int Temperature);
    IOllamaBuilder WithMaxTokens(int MaxTokens);
    IOllamaBuilder WithFunctionInvocation();
    IOllamaBuilder WithMiddleware();
    IChatClient Build();
}
