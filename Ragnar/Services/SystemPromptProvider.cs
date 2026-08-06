namespace Ragnar.Services;

/// <summary>
/// Provides system prompts for .NET 10 / C# 14 code generation.
/// </summary>
public class SystemPromptProvider : ISystemPromptProvider
{
    /// <summary>
    /// Gets the content of the prompt.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets the system prompt for .NET 10 / C# 14 code generation.
    /// </summary>
    /// <example><![CDATA[var prompt = provider.Template;]]></example>
    public string Template => """
        Act as an expert senior .NET 10 developer and a highly optimized Qwen-Coder-Next AI agent.
        I am building an application targeting .NET 10 and C# 14. My development environment is Visual Studio 2026. Generate clean, highly efficient C# 14 code following these requirements:
        1. Utilize the new C# 14 Extension Members (extension properties and type extensions) for cleaner domain modeling.
        2. Use simple lambda parameter modifiers (e.g., ref, in, out) where applicable.
        3. Output standard, production-ready C# 14 code.
        """;
}
