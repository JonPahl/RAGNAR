namespace Ragnar.Core.Model;

public static class CodeDocumentExtensions
{
    extension(CodeDocument Doc)
    {
        public IDictionary<string, Value> Dictionary => new Dictionary<string, Value> {
            { nameof(CodeDocument.FileName), Doc.FileName },{ nameof(CodeDocument.ElementType),  Doc.ElementType ?? string.Empty },
            { nameof(CodeDocument.ElementName), Doc.ElementName ?? string.Empty },
            { nameof(CodeDocument.Comment), Doc.Comment ?? string.Empty },
            { nameof(CodeDocument.Comment_Length), Doc.Comment_Length },
            { nameof(CodeDocument.Code), Doc.Code ?? string.Empty },
            { nameof(CodeDocument.Category), Doc.Category ?? "General" },
            };
    }
}
