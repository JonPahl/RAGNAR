namespace Ragnar.Embedding.Chunker;

/// <summary>
/// Parses C# syntax trees into CodeDocument chunks for embedding.
/// </summary>
/// <example><![CDATA[var docs = new SyntaxTreeChunker().ChunkSourceFile("Program.cs", code);]]></example>
public class SyntaxTreeChunker(
    Serilog.ILogger logger,
    IQdrantClient client,
    IOptions<RagnarConfig> options)
    : ISyntaxTreeChunker
{
    private const string _unknownElementName = "UNKNOWN";

    /// <summary>
    /// Parses C# syntax trees into CodeDocument chunks for embedding.
    /// </summary>
    /// <param name="filename">Source file path.</param>
    /// <param name="codeText">Full source code text.</param>
    /// <remarks>Uses Roslyn syntax tree traversal to extract top-level classes.</remarks>
    /// <example><![CDATA[var docs = new SyntaxTreeChunker().ChunkSourceFile("Program.cs", code);]]></example>
    /// <returns>List of CodeDocuments or null on parse failure.</returns>
    public async Task<IList<CodeDocument>?> ChunkSourceFile(string filename, string codeText)
    {
        var response = new List<CodeDocument>();

        var tree = CSharpSyntaxTree.ParseText(codeText);

        var r = await tree.GetRootAsync().ConfigureAwait(false);

        if (r is not CompilationUnitSyntax root)
            return null;

        foreach (var node in root.DescendantNodes())
        {
            switch (node)
            {
                case ClassDeclarationSyntax classDeclaration:
                    response.Add(LoadClass(filename, classDeclaration));
                    break;
                case InterfaceDeclarationSyntax interfaceDeclaration:
                    response.Add(LoadInterface(filename, interfaceDeclaration));
                    break;
                case RecordDeclarationSyntax recordDeclarationSyntax:
                    response.Add(LoadRecord(filename, recordDeclarationSyntax));
                    break;
                case TypeDeclarationSyntax typeDeclaration:
                    if (!await IsFileAlreadyIndexed(filename).ConfigureAwait(false))
                    {
                        response.Add(LoadTypeDeclaration(filename, typeDeclaration));
                    }
                    break;
                default:
                    break;
            }
        }

        return response;
    }

    /// <summary>Converts a generic type declaration node into a CodeDocument.</summary>
    /// <param name="filename">Source file path for the originating code element.</param>
    /// <param name="typeDeclaration">The TypeDeclarationSyntax node to serialize.</param>
    /// <returns>A CodeDocument capturing the type's code and leading comments.</returns>
    /// <example><![CDATA[var doc = SyntaxTreeChunker.LoadTypeDeclaration("X.cs", node);]]></example>
    private static CodeDocument LoadTypeDeclaration(string filename, TypeDeclarationSyntax typeDeclaration)
    {
        var category = InferCategoryFromPath(filename);

        return BuildCodeDocument(filename,
            typeDeclaration,
            typeDeclaration.Kind().ToString(),
            typeDeclaration?.Identifier.ValueText ?? _unknownElementName,
            typeDeclaration?.GetLeadingTrivia(),
            category);
    }

    /// <summary>Checks whether a file entry already exists in the Qdrant vector store.</summary>
    /// <param name="fileName">The file name to search for in the collection.</param>
    /// <returns>True if the file is stored; false if no matching point was found.</returns>
    /// <example><![CDATA[bool exists = await chunker.IsFileAlreadyIndexed("Main.cs");]]></example>
    private async Task<bool> IsFileAlreadyIndexed(string fileName)
    {
        var fileStoredFilter = new Filter
        {
            Must = {
                new Condition {
                    Field = new FieldCondition {
                        Key = "FileName",
                        Match = new Match { Keyword = fileName }}
                }}
        };

        var countResult = await client.CountAsync(
            collectionName: options.Value.ApplicationOptions.VectorStoreName,
            filter: fileStoredFilter,
            exact: true
        ).ConfigureAwait(false);

        return countResult > 0;
    }

    private static CodeDocument LoadRecord(string filename, RecordDeclarationSyntax recordDefine)
    {
        var category = InferCategoryFromPath(filename);

        return BuildCodeDocument(filename,
            recordDefine,
            recordDefine.Kind().ToString(),
            recordDefine?.Identifier.ValueText ?? _unknownElementName,
            recordDefine?.GetLeadingTrivia(),
            category);
    }

    /// <summary>
    /// Converts class declaration node to CodeDocument.
    /// </summary>
    /// <param name="filename">Source file path.</param>
    /// <param name="classDefine">Class syntax node.</param>
    /// <returns>Initialized CodeDocument.</returns>
    /// <example><![CDATA[var doc = chunker.LoadClass("Program.cs", node);]]></example>
    private static CodeDocument LoadClass(string filename, ClassDeclarationSyntax classDefine)
    {
        var category = InferCategoryFromPath(filename);

        return BuildCodeDocument(filename, classDefine, classDefine.Kind().ToString(), classDefine?.Identifier.ValueText ?? _unknownElementName, classDefine?.GetLeadingTrivia(), category);
    }

    private static CodeDocument LoadInterface(string filename, InterfaceDeclarationSyntax interfaceDefine)
    {
        var category = InferCategoryFromPath(filename);

        return BuildCodeDocument(filename, interfaceDefine, interfaceDefine.Kind().ToString(), interfaceDefine?.Identifier.ValueText ?? _unknownElementName, interfaceDefine?.GetLeadingTrivia(), category);
    }

    /// <summary>Infers a logical category label from the file name keywords.</summary>
    /// <param name="fileName">Source file name used to determine the category.</param>
    /// <returns>A category string such as Testing, Plugin, Embedding, Core, or Refactor.</returns>
    /// <example><![CDATA[string cat = SyntaxTreeChunker.InferCategoryFromPath("Tests.cs");]]></example>
    private static string InferCategoryFromPath(string fileName) => Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant() switch
    {
        var _ when fileName.ToUpperInvariant().Contains("TEST", StringComparison.OrdinalIgnoreCase) => "Testing",
        var _ when fileName.Contains("PLUGIN", StringComparison.OrdinalIgnoreCase) => "Plugin",
        var _ when fileName.Contains("EMBEDDING", StringComparison.OrdinalIgnoreCase) => "Embedding",
        var _ when fileName.Contains("CORE", StringComparison.OrdinalIgnoreCase) => "Core",
        _ => "Refactor"
    };

    /// <summary>
    /// Builds a CodeDocument from a syntax node with comments and code.
    /// </summary>
    /// <param name="filePath">Source file name.</param>
    /// <param name="node">Syntax node.</param>
    /// <param name="elementType">Element type (e.g., Class).</param>
    /// <param name="elementName">Element name.</param>
    /// <param name="leadingTrivia">Leading trivia for comments.</param>
    /// <param name="category">Category inferred from path.</param>
    /// <returns>Initialized CodeDocument.</returns>
    /// <example><![CDATA[var doc = chunker.BuildCodeDocument("Program.cs", node, "Class", "Program", trivia, "Core");]]></example>
    private static CodeDocument BuildCodeDocument(string filePath, SyntaxNode node, string elementType, string elementName, SyntaxTriviaList? leadingTrivia, string category)
    {
        var comments = ExtractDocComments(leadingTrivia);

        var x = string.Join(" ", comments);
        var commentLength = 0;
        // FIXME Call Ragnar.Extensions.StringExtensions.GetWordCount(x);

        var fileName = Path.GetFileName(filePath);

        return new CodeDocument
        {
            FileName = fileName,
            FilePath = filePath,
            ElementType = elementType,
            ElementName = elementName,
            Comment = string.Join(Environment.NewLine, comments),
            CommentLength = commentLength,
            Code = node.NormalizeWhitespace().ToFullString(),
            Category = category,
        };
    }

    /// <summary>
    /// Extracts documentation comments from leading trivia.
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia of syntax node.</param>
    /// <returns>List of trimmed comment strings.</returns>
    /// <example><![CDATA[var comments = chunker.ExtractDocComments(node.GetLeadingTrivia());]]></example>
    private static List<string> ExtractDocComments(SyntaxTriviaList? leadingTrivia)
    {
        if (leadingTrivia == null)
            return [];

        var values = leadingTrivia.Value.Where(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
        || t.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
        || t.IsKind(SyntaxKind.SingleLineCommentTrivia))
            .Select(t => t.ToString().Trim()).ToList();

        return values;
    }
}
