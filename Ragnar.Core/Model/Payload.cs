namespace Ragnar.Core.Model;

public record Payload(
    string FileName,
    string ElementType,
    string ElementName,
    string Comment,
    string Code,
    string Category);
