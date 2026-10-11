namespace Ragnar.Embedding.Pipeline.Stages;

/// <summary>Ensures the Qdrant vector store collection is initialized before retrieval.</summary>
/// <remarks>Creates the collection if it does not already exist in the target cluster.</remarks>
/// <param name="vectorSetup">The IVectorSetup service that manages collection lifecycle.</param>
/// <param name="writer">Console output writer for status messages.</param>
/// <example><![CDATA[vectorSetup.AddStage(new VectorStoreInitStage(setup, w));]]></example>
public class VectorStoreInitStage(IVectorSetup vectorSetup, IOutputWriter writer)
    : IPipelineStage<EmbeddingContext>
{
    /// <summary>Gets the human-readable display name for this vectorSetup stage.</summary>
    /// <value>The string "Setup Qdrant Vector Store".</value>
    /// <example><![CDATA[string n = stage.Name;]]>
    /// </example>
    public string Name => "Setup Qdrant Vector Store";

    public AsyncRetryPolicy? RetryPolicy => Policy.Handle<HttpRequestException>()
             .Or<TimeoutException>()
             .WaitAndRetryAsync(3, a => TimeSpan.FromSeconds(Math.Pow(2, a)));

    /// <summary>Indicates whether this stage should execute in the current vectorSetup run.</summary>
    /// <value>True to include this stage; false to skip it.</value>
    /// <example>
    /// <![CDATA[bool run = stage.ShouldRun;]]>
    /// </example>
    public bool ShouldRun(EmbeddingContext context) => true;

    /// <summary>Ensures the vector store collection exists and logs completion.</summary>
    /// <param name="context">The embedding vectorSetup context (unused directly).</param>
    /// <param name="cancellationToken">Token to abort the collection check operation.</param>
    /// <returns>A completed task once the collection is confirmed available.</returns>
    /// <example><![CDATA[await stage.ExecuteAsync(ctx, ct);]]></example>
    public async Task ExecuteAsync(EmbeddingContext context, CancellationToken cancellationToken)
    {
        await vectorSetup.EnsureCollectionExistsAsync(cancellationToken).ConfigureAwait(false);

        writer.MarkupLine("☑ Knowledge base populated.", new Style(ConsoleColor.Green));
    }
}
