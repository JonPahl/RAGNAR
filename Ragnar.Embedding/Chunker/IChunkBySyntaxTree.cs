namespace Ragnar.Embedding.Chunker;

public interface IChunkBySyntaxTree
{
    Task<IList<CodeDocument>?> ChunkSourceFile(string filename, string codeText);
}