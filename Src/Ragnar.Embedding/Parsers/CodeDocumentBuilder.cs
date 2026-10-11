namespace Ragnar.Embedding.Parsers;

/// <summary>Fluent builder for constructing <see cref="CodeDocument"/> instances.</summary>
/// <example><![CDATA[var d = new CodeDocumentBuilder().FileName("a.cs").Code("x").Build();]]></example>
public sealed class CodeDocumentBuilder
{
    /// <summary>The mutable document being constructed.</summary>
    private CodeDocument _doc = new()
    {
        FileName = string.Empty,
        FilePath = string.Empty,
        Code = string.Empty,
        Comment = string.Empty,
        CommentLength = 0,
        ElementType = "Code",
        ElementName = string.Empty,
    };

    /// <summary>Sets the file name on the document.</summary>
    /// <param name="path">The source file path.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example><![CDATA[builder.FileName("src/App.cs");]]></example>
    public CodeDocumentBuilder FileName(string path)
    {
        _doc.FileName = path;

        return this;
    }

    /// <summary>Sets the element type and name on the document.</summary>
    /// <param name="type">The syntactic element type (e.g., "method").</param>
    /// <param name="name">The element identifier name.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example><![CDATA[builder.Element("method", "Main");]]></example>
    public CodeDocumentBuilder Element(string type, string name)
    {
        _doc.ElementType = type;
        _doc.ElementName = name;

        return this;
    }

    /// <summary>Sets the source code text on the document.</summary>
    /// <param name="text">The code content string.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example><![CDATA[builder.Code("int x = 1;");]]></example>
    public CodeDocumentBuilder Code(string text)
    {
        _doc.Code = text;

        return this;
    }

    /// <summary>Sets the comment text and auto-computes its length.</summary>
    /// <param name="text">The XML comment text.</param>
    /// <returns>The builder for chaining.</returns>
    /// <example><![[CDATA[[builder.Comment("Does a thing.");]]]]></example>
    public CodeDocumentBuilder Comment(string text)
    {
        _doc.Comment = text;
        _doc.CommentLength = text.Length;

        return this;
    }

    /// <summary>Finalizes and returns the constructed CodeDocument.</summary>
    /// <returns>The completed <see cref="CodeDocument"/> instance.</returns>
    /// <example><![CDATA[var doc = builder.Build();]]>
    /// </example>
    public CodeDocument Build() => _doc;
}
