namespace PersonalKnowledge.Domain.Entities;

public class Asset : Entity
{
    public string FileName { get; set; }
    public FileExtension FileType { get; set; }
    public MediaType MediaType { get; set; }   
    public DateTime UploadedAt { get; set; }
    public int TotalChunks { get; set; }
    public string Label { get; set; }
    public Guid? TopicId { get; set; }
    public Topic Topic { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public AssetStatus Status { get; private set; } = AssetStatus.Processing;

    public void ProcessAsset()
    {
        UploadedAt = DateTime.UtcNow;
        Status = AssetStatus.Ready;
    }
}