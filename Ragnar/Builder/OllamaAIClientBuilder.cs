namespace Ragnar.Builder;

public class OllamaAIClientBuilder(
    IOllamaClientFactory clientFactory) : IOllamaAIClientBuilder
{
    private IChatClient _client;

    public OllamaAIClientBuilder WithChatClient(OllamaServiceType ollamaType)
    {
        _client = clientFactory.FindClient(ollamaType);

        return this;
    }

    public IChatClient Build()
    {

        var setup = _client
            .AsBuilder()
            .UseFunctionInvocation()
            //.UseLogging()
            .Build();

        return setup;
    }
}
