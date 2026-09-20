namespace Ragnar.Output;

public interface IContentFormatter
{
    string FileExtension { get; }
    string Format(SaveDetails details);
}
