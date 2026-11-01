namespace Ragnar.Builder;

/// <summary>
/// A builder class for constructing Rag prompts.
/// </summary>
public class RagPromptBuilder : IRagPromptBuilder
{
    /// <summary>
    /// The text of the question being built.
    /// </summary>
    private string QuestionText { get; set; }

    /// <summary>
    /// Adds a question to the prompt builder, updating the internal state with the provided question's text.
    /// </summary>
    /// <param name="question">The question to add.</param>
    /// <returns>This instance for method chaining.</returns>
    public IRagPromptBuilder WithQuestion(Question question)
    {
        QuestionText = question.Text;
        return this;
    }

    /// <summary>
    /// Builds the Rag prompt as a string, incorporating the context and question text.
    /// </summary>
    /// <param name="contextText">The context for the prompt.</param>
    /// <returns>The constructed Rag prompt.</returns>
    public string Build(string contextText)
    {
        return $$"""
          Context:
          {{contextText}}

          Question:
          {{QuestionText}}

          Answer:
          """;
    }
}
