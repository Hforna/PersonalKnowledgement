namespace PersonalKnowledge.Domain.Exceptions;

public class AssetChunkingException : AssetException
{
    public int ChunkSize { get; }
    public int Overlap { get; }

    public AssetChunkingException(string message) 
        : base(message) { }

    public AssetChunkingException(int chunkSize, int overlap, string message) 
        : base($"Error chunking asset with chunkSize={chunkSize}, overlap={overlap}: {message}") 
    {
        ChunkSize = chunkSize;
        Overlap = overlap;
    }

    public AssetChunkingException(string message, Exception innerException) 
        : base(message, innerException) { }
}

