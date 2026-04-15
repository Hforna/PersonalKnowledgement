using FileTypeChecker;
using FileTypeChecker.Types;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Exceptions;

namespace PersonalKnowledge.Application;

public interface IFileHandlerService
{
    (bool IsValid, DocumentType? DocumentType) IsValidFile(Stream file, string fileName);
    public string[] Chunk(string text, int chunkSize = 500, int overlap = 50);
}

public class FileHandlerService : IFileHandlerService
{
    private static readonly Dictionary<string, DocumentType> AllowedExtensions = new()
    {
        { ".pdf",  DocumentType.Pdf  },
        { ".txt",  DocumentType.Txt  },
        { ".md",   DocumentType.Md   },
        { ".docx", DocumentType.Docx }
    };

    public (bool IsValid, DocumentType? DocumentType) IsValidFile(Stream fileStream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!AllowedExtensions.TryGetValue(extension, out var documentType))
            return (false, null);

        if (documentType is DocumentType.Txt or DocumentType.Md)
            return (true, documentType);

        var isValidSignature = documentType switch
        {
            DocumentType.Pdf  => FileTypeValidator.Is<PortableDocumentFormat>(fileStream),
            DocumentType.Docx => FileTypeValidator.Is<ZipFile>(fileStream),
            _                 => false
        };

        fileStream.Position = 0;

        return isValidSignature
            ? (true, documentType)
            : (false, null);
    }

    public string[] Chunk(string text, int chunkSize = 500, int overlap = 50)
    {
        if (chunkSize <= 0)
            throw new DocumentChunkingException(chunkSize, overlap, "Chunk size must be greater than 0.");
        
        if (overlap < 0)
            throw new DocumentChunkingException(chunkSize, overlap, "Overlap cannot be negative.");

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<string>();
        
        if (words.Length == 0)
            return chunks.ToArray();
        
        if (overlap >= chunkSize)
            overlap = Math.Max(0, chunkSize - 1);
        
        var step = Math.Max(1, chunkSize - overlap);

        for (var i = 0; i < words.Length; i += step)
        {
            var end = Math.Min(i + chunkSize, words.Length);
            var chunk = string.Join(' ', words[i..end]);
            chunks.Add(chunk);

            if (end == words.Length)
                break;
        }

        return chunks.ToArray();
    }
}