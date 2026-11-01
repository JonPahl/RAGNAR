namespace Ragnar.Builder;

public interface IRagPromptBuilder
{
    IRagPromptBuilder WithQuestion(Question question);

    string Build(string contextText);

}
