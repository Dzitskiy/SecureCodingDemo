using Microsoft.Extensions.Caching.Memory;

namespace SecureCodingDemo.Services;

public sealed class CacheStampedeService
{
    private readonly IMemoryCache _cache;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private int _unsafeRefreshes;
    private int _safeRefreshes;

    public CacheStampedeService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public async Task<CacheReport> GetUnsafeReportAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue("unsafe-report", out CacheReport? cached) && cached is not null)
        {
            return cached;
        }

        var report = await BuildReportAsync("unsafe", Interlocked.Increment(ref _unsafeRefreshes), cancellationToken);
        _cache.Set("unsafe-report", report, TimeSpan.FromSeconds(10));
        return report;
    }

    public async Task<CacheReport> GetSafeReportAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue("safe-report", out CacheReport? cached) && cached is not null)
        {
            return cached;
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue("safe-report", out cached) && cached is not null)
            {
                return cached;
            }

            var report = await BuildReportAsync("safe", Interlocked.Increment(ref _safeRefreshes), cancellationToken);
            _cache.Set("safe-report", report, TimeSpan.FromSeconds(10));
            return report;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static async Task<CacheReport> BuildReportAsync(string mode, int refreshNumber, CancellationToken cancellationToken)
    {
        await Task.Delay(750, cancellationToken);
        return new CacheReport(mode, refreshNumber, DateTimeOffset.UtcNow);
    }
}

public sealed record CacheReport(string Mode, int RefreshNumber, DateTimeOffset GeneratedAt);
