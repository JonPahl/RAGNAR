namespace Ragnar.Core;

/// <summary>
/// Convert Qdrant value to a common dictionary.
/// </summary>
public static class BuildPayloadExtension
{
    extension(object Items)
    {
        public IReadOnlyList<IDictionary<string, Value>>? ToCollection(QdrantPayloadTypes Payload)
        {
            if(Payload == QdrantPayloadTypes.ALL && Items is (IReadOnlyList<ScoredPoint>))
            {
                return BuildAllPayload((IReadOnlyList<ScoredPoint>)Items).AsReadOnly();
            }
            if(Payload == QdrantPayloadTypes.SEARCH)
            {
                return BuildSearchPayload((RepeatedField<ScoredPoint>)Items).AsReadOnly();
            }

            return (ReadOnlyCollection<IDictionary<string, Value>>)Items;
        }
    }

    private static List<IDictionary<string, Value>> BuildSearchPayload
        (RepeatedField<ScoredPoint> Items)
    {
        var Payloads = new List<IDictionary<string, Value>>();

        foreach(var Item in Items)
        {
            Payloads.Add(Item.Payload);
        }

        return Payloads;
    }

    private static List<IDictionary<string, Value>> BuildAllPayload(IReadOnlyList<ScoredPoint> SearchResults)
    {
        var Payload = new List<IDictionary<string, Value>>();

        foreach(var Item in SearchResults)
        {
            Payload.Add(Item.Payload);
        }

        return Payload;
    }
}
