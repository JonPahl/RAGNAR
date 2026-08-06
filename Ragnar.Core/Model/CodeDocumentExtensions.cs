namespace Ragnar.Core.Model;

/// <summary>Extension methods for converting CodeDocument to Qdrant payload structures.</summary>
public static class CodeDocumentExtensions
{
    extension(CodeDocument Doc)
    {
        /// <summary>Converts the code document to a Qdrant-compatible payload dictionary.</summary>
        /// <returns>A dictionary mapping field names to Qdrant <see cref="Value"/> instances.</returns>
        /// <example><![CDATA[var payload = Doc.ToPayloadDictionary;]]></example>
        public IReadOnlyDictionary<string, Value> ToPayloadDictionary => new Dictionary<string, Value>
        {
            [nameof(Doc.FileName)] = Doc.FileName ?? string.Empty,
            [nameof(Doc.ElementType)] = Doc.ElementType ?? string.Empty,
            [nameof(Doc.ElementName)] = Doc.ElementName ?? string.Empty,
            [nameof(Doc.Comment)] = Doc.Comment ?? string.Empty,
            [nameof(Doc.CommentLength)] = Doc.CommentLength,
            [nameof(Doc.Code)] = Doc.Code ?? string.Empty,
            [nameof(Doc.Category)] = Doc.Category ?? nameof(QuestionCategory.General)
        }
        .ToImmutableDictionary(Kv => Kv.Key, Kv => Kv.Value);
    }
}
