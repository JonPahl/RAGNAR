namespace Ragnar.Factory;

/// <summary>Interface for constructing Question model instances via chaining.</summary>
public interface IQuestionBuilder
{
    /// <summary>Assigns a filename key to the question being built.</summary>
    /// <param name="key">The source filename or identifier for the question.</param>
    /// <returns>The builder instance for further configuration.</returns>
    /// <example>
    /// <![CDATA[builder.SetFileName("app.cs");]]>
    /// </example>
    IQuestionBuilder SetFileName(string key);

    /// <summary>Assigns the text content to the question being built.</summary>
    /// <param name="text">The main question or prompt text.</param>
    /// <returns>The builder instance for further configuration.</returns>
    /// <example><![CDATA[builder.SetText("Optimize this loop");]]></example>
    IQuestionBuilder SetText(string text);

    /// <summary>Sets the categorization type for the question being built.</summary>
    /// <param name="category">The classification category for the question.</param>
    /// <returns>The builder instance for further configuration.</returns>
    /// <example><![CDATA[builder.SetCategory(QuestionCategory.Refactor);]]></example>
    IQuestionBuilder SetCategory(QuestionCategory category);

    IQuestionBuilder SetActive(bool active = true);

    IQuestionBuilder SetFilter(Filter? filter = null);

    /// <summary>Builds and returns the final configured Question model instance.</summary>
    /// <returns>A fully initialized Question object.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when text or key is null or whitespace.</exception>
    /// <example><![CDATA[Question q = builder.Build();]]>
    /// </example>
    Question Build();
}
