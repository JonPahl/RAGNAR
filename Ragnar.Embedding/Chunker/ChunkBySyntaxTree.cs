namespace Ragnar.Embedding.Chunker;

/// <summary>
/// Parses C# syntax trees into CodeDocument chunks for embedding.
/// </summary>
/// <example><![CDATA[var docs = new ChunkBySyntaxTree().ChunkSourceFile("Program.cs", code);]]></example>
public static class ChunkBySyntaxTree
{
    private const string UnknownElementName = "UNKNOWN";

    /// <summary>
    /// Parses C# syntax trees into CodeDocument chunks for embedding.
    /// </summary>
    /// <param name="Filename">Source file path.</param>
    /// <param name="CodeText">Full source code text.</param>
    /// <remarks>Uses Roslyn syntax tree traversal to extract top-level classes.</remarks>
    /// <example><![CDATA[var docs = new ChunkBySyntaxTree().ChunkSourceFile("Program.cs", code);]]></example>
    /// <returns>List of CodeDocuments or null on parse failure.
    /// </returns>
    public static IList<CodeDocument>? ChunkSourceFile(string Filename, string CodeText)
    {
        var Response = new List<CodeDocument>();

        var Tree = CSharpSyntaxTree.ParseText(CodeText);

        if(Tree.GetRoot() is not CompilationUnitSyntax Root)
            return null;

        foreach(var Node in Root.DescendantNodes())
        {
            //if (node is MethodDeclarationSyntax method)
            //{
            // response.Add(LoadMethod(filename, method));
            //}
            if(Node is ClassDeclarationSyntax ClassDeclaration)
            {
                Response.Add(ExtractClassDocument(Filename, ClassDeclaration));
            }
        }

        return Response;
    }

    /// <summary>
    /// Converts class declaration node to CodeDocument.
    /// </summary>
    /// <param name="Filename">Source file path.</param>
    /// <param name="ClassDefine">Class syntax node.</param>
    /// <returns>Initialized CodeDocument.</returns>
    /// <example><![CDATA[var doc = chunker.ExtractClassDocument("Program.cs", node);]]></example>
    private static CodeDocument ExtractClassDocument(string Filename, ClassDeclarationSyntax ClassDefine)
    {
        var Category = InferCategoryFromPath(Filename);

        return CreateCodeDocument(Filename, ClassDefine, ClassDefine.Kind().ToString(), ClassDefine?.Identifier.ValueText ?? UnknownElementName, ClassDefine?.GetLeadingTrivia(), Category);
    }


    private static CodeDocument ExtractMethodDocument(string Filename, MethodDeclarationSyntax Method)
    {
        var Category = InferCategoryFromPath(Filename);

        return CreateCodeDocument(Filename, Method, Method.Kind().ToString(), Method?.Identifier.ValueText ?? UnknownElementName, Method?.GetLeadingTrivia(), Category);
    }

    private static string InferCategoryFromPath(string Path)
    {
        var FileName = System.IO.Path
            .GetFileNameWithoutExtension(Path)
            .ToLowerInvariant();

        const RegexOptions Options = default;
        return FileName switch
        {
            var _ when Regex.IsMatch(FileName, "^test", Options, TimeSpan.FromSeconds(5)) => "Testing",
            var _ when Regex.IsMatch(FileName, "^plugin", Options, TimeSpan.FromSeconds(5)) => "Plugin",
            var _ when Regex.IsMatch(FileName, "^embedding", Options, TimeSpan.FromSeconds(5)) => "Embedding",
            var _ when Regex.IsMatch(FileName, "^core", Options, TimeSpan.FromSeconds(5)) => "Core",
            _ => "Other"
        };
    }

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
    /// <example><![CDATA[var doc = chunker.CreateCodeDocument("Program.cs", node, "Class", "Program", trivia, "Core");]]>
    /// </example>
    private static CodeDocument CreateCodeDocument(string FileName, SyntaxNode Node, string ElementType, string ElementName, SyntaxTriviaList? LeadingTrivia, string Category)
    {
        var Comments = LocateComments(LeadingTrivia);

        return new CodeDocument
        (
            FileName,
            ElementType,
            ElementName,
            Comment: string.Join(Environment.NewLine, Comments),
            CommentLength: string.Join(Environment.NewLine, Comments).CharacterCount(),
            Code: Node.NormalizeWhitespace().ToFullString(),
            Category: Category
        );
    }

    /// <summary>
    /// Extracts documentation comments from leading trivia.
    /// </summary>
    /// <param name="LeadingTrivia">Leading trivia of syntax node.</param>
    /// <returns>List of trimmed comment strings.</returns>
    /// <example><![CDATA[var comments = chunker.LocateComments(node.GetLeadingTrivia());]]></example>
    private static List<string> LocateComments(SyntaxTriviaList? LeadingTrivia)
    {
        if(LeadingTrivia == null)
            return [];

        return [.. LeadingTrivia.Value.Where(T => T.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
        || T.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
        || T.IsKind(SyntaxKind.SingleLineCommentTrivia))
            .Select(T => T.ToString().Trim())];
    }
}
