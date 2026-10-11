namespace Ragnar.Core.Models;

/// <summary>Provides extension methods to convert a CodeDocument to a flat dictionary.</summary>
/// <example><![CDATA[var d = doc.Dictionary;]]></example>
public static class CodeDocumentExtensions
{
    /// <summary>Provides computed properties on CodeDocument for payload serialization.</summary>
    /// <example><![CDATA[var d = doc.Dictionary;]]></example>
    extension(CodeDocument doc)
    {

        /// <summary>Gets all CodeDocument fields as a string-to-value dictionary for payload use.</summary>
        /// <example>
        /// <![CDATA[var d = doc.Dictionary;]]>
        /// </example>
        public IDictionary<string, Value> Dictionary => new Dictionary<string, Value>
        {
            { nameof(CodeDocument.FileName), doc.FileName.Replace("//", "/", StringComparison.InvariantCultureIgnoreCase) },
            { nameof(CodeDocument.ElementType),  doc.ElementType ?? string.Empty },
            { nameof(CodeDocument.ElementName), doc.ElementName ?? string.Empty },
            { nameof(CodeDocument.Comment), doc.Comment ?? string.Empty },
            { nameof(CodeDocument.CommentLength), doc.CommentLength },
            { nameof(CodeDocument.Code), doc.Code ?? string.Empty },
            { nameof(CodeDocument.Category), doc.Category ?? "General" },
        };
    }
}
