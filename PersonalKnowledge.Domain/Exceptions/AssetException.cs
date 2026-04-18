namespace PersonalKnowledge.Domain.Exceptions;

public class AssetException : ApplicationException
{
    public AssetException(string message) : base(message) { }
    public AssetException(string message, Exception innerException) : base(message, innerException) { }
}

