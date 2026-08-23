namespace Ragnar.Embedding.Chunker;

/// <summary>
/// Parses C# syntax trees into CodeDocument chunks for embedding.
/// </summary>
/// <example><![CDATA[var docs = new ChunkBySyntaxTree().ChunkSourceFile("Program.cs", code);]]></example>
public class ChunkBySyntaxTree
{
    private const string UNKNOWN_ELEMENT_NAME = "UNKNOWN";

    /// <summary>
    /// Parses C# syntax trees into CodeDocument chunks for embedding.
    /// </summary>
    /// <param name="Filename">Source file path.</param>
    /// <param name="CodeText">Full source code text.</param>
    /// <remarks>Uses Roslyn syntax tree traversal to extract top-level classes.</remarks>
    /// <example><![CDATA[var docs = new ChunkBySyntaxTree().ChunkSourceFile("Program.cs", code);]]></example>
    /// <returns>List of CodeDocuments or null on parse failure.</returns>
    public IList<CodeDocument>? ChunkSourceFile(string Filename, string CodeText)
    {
        var response = new List<CodeDocument>();

        var tree = CSharpSyntaxTree.ParseText(CodeText);

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
                response.Add(LoadClass(Filename, classDeclaration));
            }
        }

        return response;
    }

    /// <summary>
    /// Converts class declaration node to CodeDocument.
    /// </summary>
    /// <param name="Filename">Source file path.</param>
    /// <param name="ClassDefine">Class syntax node.</param>
    /// <returns>Initialized CodeDocument.</returns>
    /// <example><![CDATA[var doc = chunker.LoadClass("Program.cs", node);]]></example>
    private CodeDocument LoadClass(string Filename, ClassDeclarationSyntax ClassDefine)
    {
        var category = InferCategoryFromPath(Filename);

        return CreateCodeDocument(Filename, ClassDefine, ClassDefine.Kind().ToString(), ClassDefine?.Identifier.ValueText ?? UNKNOWN_ELEMENT_NAME, ClassDefine?.GetLeadingTrivia(), category);
    }

    private string InferCategoryFromPath(string FileName)
    => Path.GetFileNameWithoutExtension(FileName)
        .ToLowerInvariant() switch
    {
        var _ when FileName.Contains("test", StringComparison.OrdinalIgnoreCase) => "Testing",
        var _ when FileName.Contains("plugin", StringComparison.OrdinalIgnoreCase) => "Plugin",
        var _ when FileName.Contains("embedding", StringComparison.OrdinalIgnoreCase) => "Embedding",
        var _ when FileName.Contains("core", StringComparison.OrdinalIgnoreCase) => "Core",
        _ => "Refactor"
    };

    /// <summary>
    /// Builds a CodeDocument from a syntax node with comments and code.
    /// </summary>
    /// <param name="FileName">Source file name.</param>
    /// <param name="Node">Syntax node.</param>
    /// <param name="ElementType">Element type (e.g., Class).</param>
    /// <param name="ElementName">Element name.</param>
    /// <param name="LeadingTrivia">Leading trivia for comments.</param>
    /// <param name="Category">Category inferred from path.</param>
    /// <returns>Initialized CodeDocument.</returns>
    /// <example><![CDATA[var doc = chunker.CreateCodeDocument("Program.cs", node, "Class", "Program", trivia, "Core");]]></example>
    private CodeDocument CreateCodeDocument(string FileName, SyntaxNode Node, string ElementType, string ElementName, SyntaxTriviaList? LeadingTrivia, string Category)
    {
        var comments = LocateComments(LeadingTrivia);

        return new CodeDocument
        {
            FileName = FileName,
            ElementType = ElementType,
            ElementName = ElementName,
            Comment = string.Join(Environment.NewLine, comments),
            Comment_Length = string.Join(Environment.NewLine, comments).CharacterCount(),
            Code = Node.NormalizeWhitespace().ToFullString(),
            Category = Category,
        };
    }

    /// <summary>
    /// Extracts documentation comments from leading trivia.
    /// </summary>
    /// <param name="LeadingTrivia">Leading trivia of syntax node.</param>
    /// <returns>List of trimmed comment strings.</returns>
    /// <example><![CDATA[var comments = chunker.LocateComments(node.GetLeadingTrivia());]]></example>
    private static List<string> LocateComments(SyntaxTriviaList? LeadingTrivia)
    {
        if (LeadingTrivia == null)
            return [];

        return [.. LeadingTrivia.Value.Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
        || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
        || t.IsKind(SyntaxKind.SingleLineCommentTrivia))
            .Select(t => t.ToString().Trim())];
    }
}
