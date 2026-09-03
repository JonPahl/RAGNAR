namespace Ragnar.Abstractions;

/// <summary>Provides system prompt text for the AI.</summary>
public interface IPromptProvider
{
    string System { get; }

    string GetTemplate(string content, string question);
}
