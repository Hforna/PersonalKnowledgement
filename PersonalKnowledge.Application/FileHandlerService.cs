using FileTypeChecker;
using FileTypeChecker.Types;
using PersonalKnowledge.Domain.Constants;
using PersonalKnowledge.Domain.Enums;
using PersonalKnowledge.Domain.Exceptions;

namespace PersonalKnowledge.Application;

public interface IFileHandlerService
{
    (bool IsValid, FileExtension? Extension) IsValidFile(Stream file, string fileName);
    public string[] Chunk(string text, int chunkSize = 500, int overlap = 50);
    public MediaType GetMediaType(Stream fileStream);
}

public class FileHandlerService : IFileHandlerService
{
    public (bool IsValid, FileExtension? Extension) IsValidFile(Stream fileStream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var expectedFileExtension = FileTypeIdentifiers.GetFileExtension(extension);

        if (expectedFileExtension == null)
            return (false, null);

        if (expectedFileExtension is FileExtension.Txt or FileExtension.Md)
            return (true, expectedFileExtension);

        var fileType = FileTypeValidator.GetFileType(fileStream);
        fileStream.Position = 0;

        if (fileType == null)
            return (false, null);

        var actualExtension = "." + fileType.Extension.ToLowerInvariant();
        var actualFileExtension = FileTypeIdentifiers.GetFileExtension(actualExtension);

        // Special case for docx/zip and jpeg/jpg
        if (expectedFileExtension == actualFileExtension)
            return (true, expectedFileExtension);
            
        if (expectedFileExtension == FileExtension.Docx && fileType.Extension.ToLowerInvariant() == "zip")
            return (true, expectedFileExtension);

        return (false, null);
    }

    public MediaType GetMediaType(Stream fileStream)
    {
        var fileType = FileTypeValidator.GetFileType(fileStream);
        fileStream.Position = 0;

        if (fileType == null)
            return MediaType.DOCUMENT;

        var extension = "." + fileType.Extension.ToLowerInvariant();
        var fileExtension = FileTypeIdentifiers.GetFileExtension(extension);

        return fileExtension.HasValue 
            ? FileTypeIdentifiers.GetMediaType(fileExtension.Value) 
            : MediaType.DOCUMENT;
    }

    public string[] Chunk(string text, int chunkSize = 500, int overlap = 50)
    {
        if (chunkSize <= 0)
            throw new AssetChunkingException(chunkSize, overlap, "Chunk size must be greater than 0.");
        
        if (overlap < 0)
            throw new AssetChunkingException(chunkSize, overlap, "Overlap cannot be negative.");

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