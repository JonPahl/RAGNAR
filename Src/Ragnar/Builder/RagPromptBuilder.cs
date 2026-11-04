namespace Ragnar.Builder;

public class RagPromptBuilder : IRagPromptBuilder
{
    /// <summary>
    /// The text of the question being built.
    /// </summary>
    private string QuestionText { get; set; }

    /// <summary>
    /// The retrieved context text to ground the answer.
    /// </summary>
    private string ContextText { get; set; }
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

    public IRagPromptBuilder WithContext(string context)
    {
        ContextText = context;
        return this;
    }

    /// <summary>
    /// Builds the Rag prompt as a string, incorporating the context and question text.
    /// </summary>
    /// <returns>The constructed Rag prompt.</returns>
    public string Build()
    {
        return $$"""
          Context:
          {{ContextText}}

          Question:
          {{QuestionText}}

          Answer:
          """;
    }
}
