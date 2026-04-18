namespace PersonalKnowledge.Domain.Exceptions;

public class InvalidAssetFormatException : AssetException
{
    public InvalidAssetFormatException(string fileName, string message = "Invalid asset format") 
        : base($"Asset '{fileName}' has an invalid format. {message}") { }

    public InvalidAssetFormatException(string fileName, Exception innerException) 
        : base($"Asset '{fileName}' has an invalid format.", innerException) { }
}

