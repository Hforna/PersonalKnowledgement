using PersonalKnowledge.Domain.Enums;

namespace PersonalKnowledge.Domain.Exceptions;

public class AssetHandlerNotFoundException : AssetException
{
    public FileExtension FileExtension { get; }

    public AssetHandlerNotFoundException(FileExtension fileExtension) 
        : base($"No parser service found for asset extension '{fileExtension}'.") 
    {
        FileExtension = fileExtension;
    }
}

