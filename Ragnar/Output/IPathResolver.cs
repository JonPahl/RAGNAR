namespace Ragnar.Output;

public interface IPathResolver
{
    string ResolveResponseDirectory(QuestionCategory? category);
}

public interface IContentFormatter
{
    string FileExtension { get; }
    string Format(SaveDetails details);
}
