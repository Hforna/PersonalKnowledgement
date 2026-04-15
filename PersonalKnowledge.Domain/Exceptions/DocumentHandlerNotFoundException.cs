using PersonalKnowledge.Domain.Enums;

namespace PersonalKnowledge.Domain.Exceptions;

public class DocumentHandlerNotFoundException : DocumentException
{
    public DocumentType DocumentType { get; }

    public DocumentHandlerNotFoundException(DocumentType documentType) 
        : base($"No parser service found for document type '{documentType}'.") 
    {
        DocumentType = documentType;
    }
}

