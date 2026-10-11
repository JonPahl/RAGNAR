namespace Ragnar.Embedding.Pipeline;

/// <summary>Initializes a new instance of the embedding pipeline.</summary>
/// <param name = "logger"> Logger for diagnostic messages.</param>
/// <param name = "writer"> Output writer for console feedback.</param>
public class VectorSetup(
    Serilog.ILogger logger,
    IVectorStoreBuilder vectorStoreBuilder,
    IOutputWriter writer)
    : IVectorSetup
{
    /// <summary>
    /// Ensures target vector collection exists; creates if not found.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task.</returns>
    /// <example><![CDATA[await EnsureCollectionExistsAsync(ct);]]></example>
    public async Task EnsureCollectionExistsAsync(CancellationToken cancellationToken)
    {
        IEnumerable<(string Field, PayloadSchemaType Schema)>? fields =
        [
            ("file_path", PayloadSchemaType.Keyword),
            ("file_name", PayloadSchemaType.Keyword),
            ("file_extension", PayloadSchemaType.Keyword),
            ("file_size", PayloadSchemaType.Integer),
            ("created_at", PayloadSchemaType.Datetime),
            ("modified_at", PayloadSchemaType.Datetime)
        ];

        var collectionExists = await vectorStoreBuilder.BuildAsync(fields, cancellationToken).ConfigureAwait(false);

        //.BuildAsync(cancellationToken).ConfigureAwait(false);

        if (!collectionExists)
        {
            writer.MarkupLine("[green] ☑ Collection Created [/]");
            logger.Information("Collection Created.");
        }

        writer.MarkupLine("[green] ☑ Collection Exists [/]");
        logger.Information("Collection Exists.");
    }
}
