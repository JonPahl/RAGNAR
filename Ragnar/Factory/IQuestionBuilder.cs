namespace Ragnar.Factory;

/// <summary>Interface for constructing Question model instances via chaining.</summary>
public interface IQuestionBuilder
{
    /// <summary>Sets the question status to active and returns the builder.</summary>
    /// <returns>The builder instance for further configuration.</returns>
    QuestionBuilder AsActive();

    /// <summary>Sets the question status to inactive and returns the builder.</summary>
    /// <returns>The builder instance for further configuration.</returns>
    QuestionBuilder AsInactive();

    /// <summary>Assigns a filename key to the question being built.</summary>
    /// <param name="key">The source filename or identifier for the question.</param>
    /// <returns>The builder instance for further configuration.</returns>
    QuestionBuilder WithFileName(string key);

    /// <summary>Assigns the text content to the question being built.</summary>
    /// <param name="text">The main question or prompt text.</param>
    /// <returns>The builder instance for further configuration.</returns>
    QuestionBuilder WithText(string text);

    /// <summary>Sets the categorization type for the question being built.</summary>
    /// <param name="category">The classification category for the question.</param>
    /// <returns>The builder instance for further configuration.</returns>
    QuestionBuilder SetCategory(QuestionCategory? category);

    /// <summary>Builds and returns the final configured Question model instance.</summary>
    /// <returns>A fully initialized Core.Model.Question object.</returns>
    Core.Model.Question Build();
}
