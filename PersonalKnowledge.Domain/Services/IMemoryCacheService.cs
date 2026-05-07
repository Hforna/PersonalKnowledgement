namespace PersonalKnowledge.Domain.Services;

public interface IMemoryCacheService
{
    public string GetValueByKey(string? key);
    public bool SetValue(string key, string value);
}