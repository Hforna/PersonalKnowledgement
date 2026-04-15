namespace PersonalKnowledge.Domain.Services;

public interface IDocumentProcessing
{
    public Task ProcessDocument(Guid documentId);
}