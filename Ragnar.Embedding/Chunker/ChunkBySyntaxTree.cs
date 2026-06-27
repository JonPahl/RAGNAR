
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ragnar.Core.Model;

namespace Ragnar.Embedding.Chunker;
/// <summary>
/// Parses C# syntax trees into CodeDocument chunks for embedding.
/// </summary>
/// <example><![CDATA[var docs = new ChunkBySyntaxTree().ChunkSourceFile("Program.cs", code);]]></example>
public class ChunkBySyntaxTree
{
    private const string _unknownElementName = "UNKNOWN";

    /// <summary>
    /// Parses C# syntax trees into CodeDocument chunks for embedding.
    /// </summary>
    /// <param name="filename">Source file path.</param>
    /// <param name="codeText">Full source code text.</param>
    /// <remarks>Uses Roslyn syntax tree traversal to extract top-level classes.</remarks>
    /// <example><![CDATA[var docs = new ChunkBySyntaxTree().ChunkSourceFile("Program.cs", code);]]></example>
    /// <returns>List of CodeDocuments or null on parse failure.</returns>
    public IList<CodeDocument>? ChunkSourceFile(string filename, string codeText)
    {
        var response = new List<CodeDocument>();

        var tree = CSharpSyntaxTree.ParseText(codeText);

        if (tree.GetRoot() is not CompilationUnitSyntax root)
            return null;

        foreach (var node in root.DescendantNodes())
        {
            //if (node is MethodDeclarationSyntax method)
            //{
            // response.Add(LoadMethod(filename, method));
            //}
            if (node is ClassDeclarationSyntax classDeclaration)
            {
                response.Add(LoadClass(filename, classDeclaration));
            }
        }

        return response;
    }

    /// <summary>
    /// Converts class declaration node to CodeDocument.
    /// </summary>
    /// <param name="filename">Source file path.</param>
    /// <param name="classDefine">Class syntax node.</param>
    /// <returns>Initialized CodeDocument.</returns>
    /// <example><![CDATA[var doc = chunker.LoadClass("Program.cs", node);]]></example>
    private CodeDocument LoadClass(string filename, ClassDeclarationSyntax classDefine)
    {
        var category = InferCategoryFromPath(filename);
        return CreateCodeDocument(filename, classDefine, classDefine.Kind().ToString(), classDefine?.Identifier.ValueText ?? _unknownElementName, classDefine?.GetLeadingTrivia(), category);
    }


    private CodeDocument LoadMethod(string filename, MethodDeclarationSyntax method)
    {
        var category = InferCategoryFromPath(filename);
        return CreateCodeDocument(filename, method, method.Kind().ToString(), method?.Identifier.ValueText ?? _unknownElementName, method?.GetLeadingTrivia(), category);
    }

    private string InferCategoryFromPath(string path)
    => Path.GetFileNameWithoutExtension(path)
        .ToLowerInvariant() switch
    {
        var _ when path.Contains("test", StringComparison.OrdinalIgnoreCase) => "Testing",
        var _ when path.Contains("plugin", StringComparison.OrdinalIgnoreCase) => "Plugin",
        var _ when path.Contains("embedding", StringComparison.OrdinalIgnoreCase) => "Embedding",
        var _ when path.Contains("core", StringComparison.OrdinalIgnoreCase) => "Core",
        _ => "Refactor"
    };

    /// <summary>
    /// Builds a CodeDocument from a syntax node with comments and code.
    /// </summary>
    /// <param name="fileName">Source file name.</param>
    /// <param name="node">Syntax node.</param>
    /// <param name="elementType">Element type (e.g., Class).</param>
    /// <param name="elementName">Element name.</param>
    /// <param name="leadingTrivia">Leading trivia for comments.</param>
    /// <param name="category">Category inferred from path.</param>
    /// <returns>Initialized CodeDocument.</returns>
    /// <example><![CDATA[var doc = chunker.CreateCodeDocument("Program.cs", node, "Class", "Program", trivia, "Core");]]></example>
    private CodeDocument CreateCodeDocument(string fileName, SyntaxNode node, string elementType, string elementName, SyntaxTriviaList? leadingTrivia, string category)
    {
        var comments = LocateComments(leadingTrivia);

        return new CodeDocument
        {
            FileName = fileName,
            ElementType = elementType,
            ElementName = elementName,
            Comment = string.Join(Environment.NewLine, comments),
            Comment_Length = string.Join(Environment.NewLine, comments).CharacterCount(),
            Code = node.NormalizeWhitespace().ToFullString(),
            Category = category,
        };
    }

    /// <summary>
    /// Extracts documentation comments from leading trivia.
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia of syntax node.</param>
    /// <returns>List of trimmed comment strings.</returns>
    /// <example><![CDATA[var comments = chunker.LocateComments(node.GetLeadingTrivia());]]></example>
    private static List<string> LocateComments(SyntaxTriviaList? leadingTrivia)
    {
        if (leadingTrivia == null)
            return [];

        return [.. leadingTrivia.Value.Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
        || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
        || t.IsKind(SyntaxKind.SingleLineCommentTrivia))
            .Select(t => t.ToString().Trim())];
    }
}
