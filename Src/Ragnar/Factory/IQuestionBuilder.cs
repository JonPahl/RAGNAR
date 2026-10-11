namespace Ragnar.Factory;

/// <summary>Interface for constructing Question model instances via chaining.</summary>
public interface IQuestionBuilder
{
    /// <summary>Sets the question status to active and returns the builder.</summary>
    /// <returns>The builder instance for further configuration.</returns>
    /// <example><![CDATA[builder.AsActive();]]></example>
    QuestionBuilder AsActive();

    /// <summary>Sets the question status to inactive and returns the builder.</summary>
    /// <returns>The builder instance for further configuration.</returns>
    /// <example><![CDATA[builder.AsInactive();]]></example>
    QuestionBuilder AsInactive();

    /// <summary>Assigns a filename key to the question being built.</summary>
    /// <param name="key">The source filename or identifier for the question.</param>
    /// <returns>The builder instance for further configuration.</returns>
    /// <example>
    /// <![CDATA[builder.WithFileName("app.cs");]]>
    /// </example>
    QuestionBuilder WithFileName(string key);

    /// <summary>Assigns the text content to the question being built.</summary>
    /// <param name="text">The main question or prompt text.</param>
    /// <returns>The builder instance for further configuration.</returns>
    /// <example><![CDATA[builder.WithText("Optimize this loop");]]></example>
    QuestionBuilder WithText(string text);

    /// <summary>Sets the categorization type for the question being built.</summary>
    /// <param name="category">The classification category for the question.</param>
    /// <returns>The builder instance for further configuration.</returns>
    /// <example><![CDATA[builder.SetCategory(QuestionCategory.Refactor);]]></example>
    QuestionBuilder SetCategory(QuestionCategory? category);

    /// <summary>Builds and returns the final configured Question model instance.</summary>
    /// <returns>A fully initialized Question object.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when text or key is null or whitespace.</exception>
    /// <example><![CDATA[Question q = builder.Build();]]>
    /// </example>
    Question Build();
}
