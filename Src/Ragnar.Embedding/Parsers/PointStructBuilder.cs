//using System.Reflection.Metadata;

//namespace Ragnar.Embedding.Parsers;

//public record PointStructBuilder
//{
//    // Make this init-only or public so the with-expression can clone it safely
//    private Dictionary<string, object?> _payload { get; init; } = [];

//    // FIXED: Use the C# 'with' expression syntax 
//    public PointStructBuilder Id(PointId id) => this with { _payload = CloneWith("id", id) };
//    public PointStructBuilder Vectors(float[] v) => this with { _payload = CloneWith("vectors", v) };

//    public PointStructBuilder Payload(string key, object? value)
//        => this with { _payload = CloneWith(key, value) };

//    internal object? Get(string k) => _payload.GetValueOrDefault(k);

//    public PointStruct Build() => new()
//    {
//        Id = (PointId)Get("id")!,
//        Vectors = (float[])Get("vectors")!,
//        Payload = { _payload.Dictionary }
//    }

//        /*
//         return [new PointStruct
//        {
//            Id = pointId,
//            Vectors = embedding,
//            Payload = { document.Dictionary }
//        }];
//         */

//// Helper to keep dictionaries isolated between record mutations
//private Dictionary<string, object?> CloneWith(string key, object? value) => new(_payload) { [key] = value };
//}
