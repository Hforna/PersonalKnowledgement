namespace PersonalKnowledge.Domain.Entities;

public class Document : Entity
{
    public string FileName { get; set; }
    public DocumentType FileType { get; set; }
    public DateTime UploadedAt { get; set; }
    public int TotalChunks { get; set; }
    public Guid? TopicId { get; set; }
    public Topic Topic { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public DocumentStatus Status { get; private set; } = DocumentStatus.Processing;

    public void ProcessDocument()
    {
        UploadedAt = DateTime.UtcNow;
        Status = DocumentStatus.Ready;
    }
}