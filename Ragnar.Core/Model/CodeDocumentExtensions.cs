namespace Ragnar.Core.Model;


/// <summary>Converts a CodeDocument to a dictionary of payload fields for Qdrant.</summary>
public static class CodeDocumentExtensions
{
    extension(CodeDocument doc)
    {
        public IDictionary<string, Value> ToDictionary
            => new Dictionary<string, Value>
            {
                { nameof(CodeDocument.FileName), doc.FileName },
                { nameof(CodeDocument.ElementType),  doc.ElementType ?? string.Empty },
                { nameof(CodeDocument.ElementName), doc.ElementName ?? string.Empty },
                { nameof(CodeDocument.Comment), doc.Comment ?? string.Empty },
                { nameof(CodeDocument.Comment_Length), doc.Comment_Length },
                { nameof(CodeDocument.Code), doc.Code ?? string.Empty },
                { nameof(CodeDocument.Category), doc.Category ?? "General" },
            };
    }
}
