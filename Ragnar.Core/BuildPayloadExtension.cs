namespace Ragnar.Core;

/// <summary>
/// Convert Qdrant value to a common dictionary.
/// </summary>
public static class BuildPayloadExtension
{
    extension(object items)
    {
        public IReadOnlyList<IDictionary<string, Value>>? Build (QdrantPayloadTypes payload)
        {
            if (payload == QdrantPayloadTypes.ALL && items is (IReadOnlyList<ScoredPoint>))
            {
                return BuildAllPayload((IReadOnlyList<ScoredPoint>)items).AsReadOnly();
            }
            if (payload == QdrantPayloadTypes.SEARCH && items is (RepeatedField<RetrievedPoint>))
            {
                return BuildSearchPayload((RepeatedField<RetrievedPoint>)items).AsReadOnly();
            }

            var a = items.GetType().Name;
            return (ReadOnlyCollection<IDictionary<string, Value>>)items;
        }
    }

    private static IList<IDictionary<string, Value>> BuildSearchPayload
        (RepeatedField<RetrievedPoint> items)
    {
        var payloads = new List<IDictionary<string, Value>>();

        foreach (var item in items)
        {
            payloads.Add(item.Payload);
        }

        return payloads;
    }

    private static IList<IDictionary<string, Value>> BuildAllPayload (IReadOnlyList<ScoredPoint> searchResults)
    {
        var payload = new List<IDictionary<string, Value>>();

        foreach (var item in searchResults)
        {
            payload.Add(item.Payload);
        }

        return payload;
    }
}
