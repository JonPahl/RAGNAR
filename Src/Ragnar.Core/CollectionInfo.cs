namespace Ragnar.Core;

/// <summary>Immutable snapshot of a Qdrant collection's metadata.</summary>
/// <example><![[CDATA[[var info = new CollectionInfo { Name = "ragnar", Dimension = 768 };]]]]></example>
public sealed record CollectionInfo
{
    /// <summary>Name of the vector collection.</summary>
    /// <example><![CDATA[string n = info.Name;]]></example>
    public required string Name { get; init; }

    /// <summary>Vector dimension of the collection's points.</summary>
    /// <example><![CDATA[int d = info.Dimension; // 768]]></example>
    public required int Dimension { get; init; }

    /// <summary>Total number of vectors stored in the collection.</summary>
    /// <example><![CDATA[int c = info.VectorsCount; // 1024]]></example>
    public required int VectorsCount { get; init; }

    /// <summary>Current operational status of the collection (e.g. "green").</summary>
    /// <example><![CDATA[string s = info.Status; // "green"]]></example>
    public required string Status { get; init; }
}

