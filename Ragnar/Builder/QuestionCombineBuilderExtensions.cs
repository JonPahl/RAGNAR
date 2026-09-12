namespace Ragnar.Builder;

public static class QuestionCombineBuilderExtensions
{

    /// <summary>Registers embedding and question-plugin services into the DI container.</summary>
    /// <remarks>Loads plugin DLLs at runtime from the Questions/Plugins folder.</remarks>
    /// <example><![CDATA[services.RegisterEmbeddingServices().LoadQuestionPlugins();]]></example>

    extension(QuestionBuilder builder)
    {
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
