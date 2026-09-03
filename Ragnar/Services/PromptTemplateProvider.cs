namespace Ragnar.Services;

/// <summary>Initializes a new instance of the system prompt provider.</summary>
public class PromptTemplateProvider
    : IPromptProvider
{
    /// <summary> Gets the content of the prompt. </summary>

    public string GetTemplate(string content, string question) => $"{content}\r\nQuestion: {question}";

    /// <summary>Gets the system prompt for .NET 10 code generation.</summary>
    /// <returns>The prompt template string for AI agent instructions.</returns>
    /// <example><![CDATA[var t = provider.Template;]]></example>
    public string System => """
        Act as an expert senior .NET 10 developer and a highly optimized Qwen coding AI agent.
        I am building an application targeting .NET 10 and C# 14. My development environment is Visual Studio 2026. Generate clean, highly efficient C# 14 code following these requirements:
        1. Utilize the new C# 14 Extension Members (extension properties and type extensions) for cleaner domain modeling.
        2. Use simple lambda parameter modifiers (e.g., ref, in, out) where applicable.
        3. Output standard, production-ready C# 14 code.
        """;
}
