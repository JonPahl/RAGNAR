namespace Ragnar.Builder;

/// <summary>
/// Fluent builder for constructing Retrieval-Augmented Generation (RAG) prompts. Composes a question with retrieved context into a structured prompt string suitable for LLM consumption. </summary>
/// <example><![CDATA[var p = builder.WithQuestion(q).WithContext("ctx").Build();]></example>
public interface IRagPromptBuilder
{
    /// <summary>Sets the user's question to be answered by the model.</summary>
    /// <param name="question">The natural-language query from the end user.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <example><![CDATA[builder.WithQuestion(question);]></example>
    IRagPromptBuilder WithQuestion(Question question);

    /// <summary>Sets the retrieved context (e.g., concatenated document chunks) to ground the model's response.</summary>
    /// <param name="context">Pre-formatted text containing relevant source material.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <example><![CDATA[builder.WithContext("retrieved text");]]></example>
    IRagPromptBuilder WithContext(string context);

    /// <summary>Assembles and returns the final prompt string from all previously configured parts.</summary>
    /// <remarks>Call after at least one of <see cref="WithQuestion"/> or <see cref="WithContext"/>.</remarks>
    /// <returns>A fully composed prompt ready for submission to an LLM endpoint.</returns>
    /// <example><![CDATA[string prompt = builder.Build();]></example>
    string Build();
}
