using LoanApplication.Application.Common.Contracts.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace LoanApplication.Infrastructure.Services;

internal sealed class UtilityService(
    IMemoryCache memoryCache) : IUtilityService
{
    #region Cache

    public void SetInMemoryCache<TItem>(object key, TItem value, TimeSpan absoluteExpirationRelativeToNow)
        => memoryCache.Set(key, value,
            TimeSpan.FromSeconds(Math.Max(absoluteExpirationRelativeToNow.TotalSeconds,
                absoluteExpirationRelativeToNow.TotalSeconds - 60)));

    public void SetInMemoryCache<TItem>(object key, TItem value, MemoryCacheEntryOptions options)
    {
        memoryCache.Set(key, value, options);
    }

    public bool TryGetInMemoryCacheValue<TItem>(string key, out TItem? value)
        => memoryCache.TryGetValue(key, out value);

    public void RemoveInMemoryCache(object key) => memoryCache.Remove(key);

    #endregion
}