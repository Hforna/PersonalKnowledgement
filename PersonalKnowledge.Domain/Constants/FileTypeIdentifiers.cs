using PersonalKnowledge.Domain.Enums;

namespace PersonalKnowledge.Domain.Constants;

public static class FileTypeIdentifiers
{
    public static readonly IReadOnlyDictionary<FileExtension, MediaType> ExtensionCategoryMap = new Dictionary<FileExtension, MediaType>
    {
        { FileExtension.Pdf, MediaType.DOCUMENT },
        { FileExtension.Txt, MediaType.DOCUMENT },
        { FileExtension.Md, MediaType.DOCUMENT },
        { FileExtension.Docx, MediaType.DOCUMENT },
        { FileExtension.Png, MediaType.IMAGE },
        { FileExtension.Jpg, MediaType.IMAGE },
        { FileExtension.Gif, MediaType.IMAGE },
        { FileExtension.Bmp, MediaType.IMAGE },
        { FileExtension.Webp, MediaType.IMAGE },
        { FileExtension.Mp4, MediaType.VIDEO },
        { FileExtension.Avi, MediaType.VIDEO },
        { FileExtension.Mkv, MediaType.VIDEO },
        { FileExtension.Webm, MediaType.VIDEO },
        { FileExtension.Mp3, MediaType.AUDIO },
        { FileExtension.Wav, MediaType.AUDIO },
        { FileExtension.Ogg, MediaType.AUDIO },
        { FileExtension.M4a, MediaType.AUDIO },
        { FileExtension.Doc, MediaType.DOCUMENT },
        { FileExtension.Mov, MediaType.VIDEO },
        { FileExtension.Tiff, MediaType.IMAGE }
    };

    public static readonly IReadOnlyDictionary<FileExtension, string> ExtensionMimeTypeMap = new Dictionary<FileExtension, string>
    {
        { FileExtension.Pdf, "application/pdf" },
        { FileExtension.Txt, "text/plain" },
        { FileExtension.Md, "text/markdown" },
        { FileExtension.Docx, "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { FileExtension.Doc, "application/msword" },
        { FileExtension.Png, "image/png" },
        { FileExtension.Jpg, "image/jpeg" },
        { FileExtension.Gif, "image/gif" },
        { FileExtension.Bmp, "image/bmp" },
        { FileExtension.Webp, "image/webp" },
        { FileExtension.Tiff, "image/tiff" },
        { FileExtension.Mp4, "video/mp4" },
        { FileExtension.Avi, "video/x-msvideo" },
        { FileExtension.Mkv, "video/x-matroska" },
        { FileExtension.Webm, "video/webm" },
        { FileExtension.Mov, "video/quicktime" },
        { FileExtension.Mp3, "audio/mpeg" },
        { FileExtension.Wav, "audio/wav" },
        { FileExtension.Ogg, "audio/ogg" },
        { FileExtension.M4a, "audio/mp4" }
    };

    public static readonly IReadOnlyDictionary<string, FileExtension> ExtensionMap = new Dictionary<string, FileExtension>
    {
        { ".pdf", FileExtension.Pdf },
        { ".txt", FileExtension.Txt },
        { ".md", FileExtension.Md },
        { ".docx", FileExtension.Docx },
        { ".doc", FileExtension.Doc },
        { ".png", FileExtension.Png },
        { ".jpg", FileExtension.Jpg },
        { ".jpeg", FileExtension.Jpg },
        { ".gif", FileExtension.Gif },
        { ".bmp", FileExtension.Bmp },
        { ".webp", FileExtension.Webp },
        { ".tiff", FileExtension.Tiff },
        { ".mp4", FileExtension.Mp4 },
        { ".avi", FileExtension.Avi },
        { ".mkv", FileExtension.Mkv },
        { ".webm", FileExtension.Webm },
        { ".mov", FileExtension.Mov },
        { ".mp3", FileExtension.Mp3 },
        { ".wav", FileExtension.Wav },
        { ".ogg", FileExtension.Ogg },
        { ".m4a", FileExtension.M4a }
    };

    public static bool IsValidExtension(string extension)
    {
        return ExtensionMap.ContainsKey(extension.ToLowerInvariant());
    }

    public static FileExtension? GetFileExtension(string extension)
    {
        return ExtensionMap.TryGetValue(extension.ToLowerInvariant(), out var fileExtension) ? fileExtension : null;
    }

    public static MediaType GetMediaType(FileExtension extension)
    {
        return ExtensionCategoryMap.TryGetValue(extension, out var mediaType) ? mediaType : MediaType.DOCUMENT;
    }

    public static string GetMimeType(FileExtension extension)
    {
        return ExtensionMimeTypeMap.TryGetValue(extension, out var mimeType) ? mimeType : "application/octet-stream";
    }
}