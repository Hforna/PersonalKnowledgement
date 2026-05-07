using Microsoft.Extensions.Caching.Memory;
using PersonalKnowledge.Domain.Services;

namespace PersonalKnowledge.Infrastructure.Services;

public class MemoryCacheService : IMemoryCacheService
{
    private readonly IMemoryCache _memoryCache;

    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public string? GetValueByKey(string key)
    {
        return _memoryCache.TryGetValue(key, out string? value) ? value : "";
    }

    public bool SetValue(string key, string value)
    {
        var returnValue = _memoryCache.Set(key, value);

        return !string.IsNullOrEmpty(returnValue);
    }
}