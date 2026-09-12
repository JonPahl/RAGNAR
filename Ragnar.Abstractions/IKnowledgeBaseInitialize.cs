namespace Ragnar.Abstractions;

/// <summary>Interface for initializing and populating the vector knowledge base.</summary>
/// <example><![CDATA[await kb.InitializeVectorStoreAsync(ct);]]></example>
public interface IKnowledgeBaseInitialize
{
    /// <summary>Ensures the target vector store collection exists.</summary>
    /// <param name="cancellationToken">Token to cancel the initialization operation.</param>
    /// <returns>A task representing the asynchronous initialization result.</returns>
    /// <example><![CDATA[await kb.InitializeVectorStoreAsync(ct);]]></example>
    ValueTask InitializeVectorStoreAsync(CancellationToken cancellationToken);

    /// <summary>Runs the full embedding pipeline to populate the knowledge base.</summary>
    /// <param name="cancellationToken">Token to cancel the pipeline execution.</param>
    /// <returns>A task representing the async pipeline completion.</returns>
    /// <example><![CDATA[await kb.RunEmbeddingPipelineAsync(ct);]]></example>
    Task RunEmbeddingPipelineAsync(CancellationToken cancellationToken);

    /// <summary>Processes the loaded questions against the vector store.</summary>
    /// <param name="cancellationToken">Token to cancel the question processing.</param>
    /// <returns>A task representing the async question answering result.</returns>
    /// <example><![CDATA[await kb.AskQuestionsAsync(ct);]]></example>
    Task AskQuestionsAsync(CancellationToken cancellationToken);
}
