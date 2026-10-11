namespace Ragnar.Builder;

/// <summary>Provides fluent convenience methods for <see cref="QuestionBuilder"/>.</summary>
public static class QuestionCombineBuilderExtensions
{

    /// <summary>Registers embedding and question-plugin services into the DI container.</summary>
    /// <remarks>Loads plugin DLLs at runtime from the Questions/Plugins folder.</remarks>
    /// <example><![CDATA[services.RegisterEmbeddingServices().LoadQuestionPlugins();]]></example>
    extension(QuestionBuilder builder)
    {
        /// <summary>Sets the question's active state in a single fluent call.</summary>
        /// <param name="active">Whether the question should be active.</param>
        /// <returns>The builder instance for further configuration.</returns>
        /// <example><![CDATA[builder.SetActive(true);]]></example>

        public QuestionBuilder SetActive(bool active)
        {
            if (active)
            {
                builder.AsActive();
            }
            else
            {
                builder.AsInactive();
            }

            return builder;
        }
    }
}
