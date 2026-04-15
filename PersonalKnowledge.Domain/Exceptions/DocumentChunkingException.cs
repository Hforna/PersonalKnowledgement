namespace PersonalKnowledge.Domain.Exceptions;

public class DocumentChunkingException : DocumentException
{
    public int ChunkSize { get; }
    public int Overlap { get; }

    public DocumentChunkingException(string message) 
        : base(message) { }

    public DocumentChunkingException(int chunkSize, int overlap, string message) 
        : base($"Error chunking document with chunkSize={chunkSize}, overlap={overlap}: {message}") 
    {
        ChunkSize = chunkSize;
        Overlap = overlap;
    }

    public DocumentChunkingException(string message, Exception innerException) 
        : base(message, innerException) { }
}

