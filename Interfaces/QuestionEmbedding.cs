using System.Text;

using Microsoft.SemanticKernel.Embeddings;

namespace RAGNAR.Interfaces;

public class QuestionEmbedding(Serilog.ILogger logger, IOptions<ApplicationConfiguration> configWrapper, IGeneratorService embeddingService, IQdrantClient qdrantClient, IOllamaClientProvider clientFactory) : IQuestionEmbedding
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> generator = clientFactory.FindClient(OllamaType.Embedding).AsEmbeddingGenerator();

    /// <summary>
    /// Retrieves top-k context from Qdrant using embedding query vector.
    /// </summary>
    /// <param name="VectorStoreName">Qdrant collection name.</param>
    /// <param name="QuestionEmbeddingVector">Embedding vector of user query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="filter">Optional qdrant filter.</param>
    /// <returns>Aggregated context strings.</returns>
    /// <example><![CDATA[string ctx = await GetContext("docs", qVec, ct);]]></example>
    public async Task<string> GetContext(
        string VectorStoreName,
        ReadOnlyMemory<float> QuestionEmbeddingVector,
        CancellationToken ct, Filter? filter = null)
    {
        var expectedDim = configWrapper.Value.EmbeddingOptions.Dimension;
        if (Convert.ToUInt64(QuestionEmbeddingVector.Length) != expectedDim)
        {
            throw new ArgumentException($"Query vector dimension {QuestionEmbeddingVector.Length} ≠ expected {expectedDim}", nameof(QuestionEmbeddingVector));
        }

        /* TODO: Can I make a filter object be an expression tree? Example of expression. var emptyOrNullFilter = new Filter { Should = {
        new Condition {
        Field = new FieldCondition {
        Key = "Comment",Match = new Match { Text = "" }}},
        new Condition { IsEmpty = new IsEmptyCondition {Key = "Comment"}}}}; */

        // TODO: Make filter configurable and able to be added to exiting Question object.

        var searchResult = await qdrantClient.SearchAsync(
            collectionName: VectorStoreName,
            vector: QuestionEmbeddingVector,
            // filter: filter,
            limit: 100,
            cancellationToken: ct);

        return await StreamContextAsync([.. searchResult]);
    }

    /// <summary>
    /// Generates embedding vector for given text using configured model.
    /// </summary>
    /// <param name="userQuestion">Input text to embed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Embedding vector as ReadOnlyMemory&lt;float&gt;.</returns>
    /// <example><![CDATA[var vec = await GenerateEmbeddingAsync(ollama, "Query?", ct);]]></example>
    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string userQuestion,
        CancellationToken ct)
    {
        var qv = await embeddingService.GenerateEmbeddingsAsync(logger, generator, userQuestion, ct);

        return qv[0].Vector;
    }

    /// <summary>
    /// Aggregates Qdrant search payloads into a context string.
    /// </summary>
    /// <param name="searchResult">Qdrant search results.</param>
    /// <returns>Concatenated code context with [CONTEXT CODE] tags.</returns>
    /// <example><![CDATA[string ctx = await StreamContextAsync(results);]]></example>
    private static async Task<string> StreamContextAsync(IReadOnlyList<ScoredPoint> searchResult)
    {
        var sb = new StringBuilder();

        sb.AppendLine("[CONTEXT CODE]");

        const string FILE_NAME = "file_name";
        const string ELEMENT_NAME = nameof(CodeDocument.ElementName);
        const string COMMENT = nameof(CodeDocument.Comment);
        const string CODE = nameof(CodeDocument.Code);
        const string ELEMENT_TYPE = nameof(CodeDocument.ElementType);

        foreach (var match in searchResult.Select(p => p.Payload))
        {
            var fileName = match.TryGetValue(FILE_NAME, out var fn) ? fn.StringValue ?? string.Empty : string.Empty;

            var elementName = match.TryGetValue(ELEMENT_NAME, out var en) ? en.StringValue ?? string.Empty : string.Empty;

            var comment = match.TryGetValue(COMMENT, out var cmt) ? cmt.StringValue ?? string.Empty : string.Empty;

            var code = match.TryGetValue(CODE, out var cd) ? cd.StringValue ?? string.Empty : string.Empty;

            var elementType = match.TryGetValue(ELEMENT_TYPE, out var et) ? et.StringValue ?? string.Empty : string.Empty;

            sb.AppendLine($"File Name: {fileName} Type: {elementType} Element Name: {elementName} Description: {comment} Code: {code}");
        }

        sb.AppendLine("[/CONTEXT CODE]");
        return sb.ToString();
    }
}
