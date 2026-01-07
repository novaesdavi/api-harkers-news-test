using HackerNews.Domain.Entities;
using HackerNews.Infra.Services;
using Serilog;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNews.Infra.Repositories;

public class CachedStoryRepository : IStoryRepository
{
    private readonly IStoryRepository _inner;
    private readonly IHackerNewsApi _api;
    private readonly ILogger _logger;
    private readonly IMemoryCache _cache;

    public CachedStoryRepository(IStoryRepository inner, IHackerNewsApi api, ILogger logger, IMemoryCache cache)
    {
        _inner = inner;
        _api = api;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<Story>> BestStoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var ids = await _cache.GetOrCreateAsync("best_ids", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return await _api.GetBestStoriesIds();
            });

            var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2);
            var tasks = ids.Select(async id =>
            {
                var cacheKey = $"item_{id}";
                return await _cache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        var item = await _api.GetItem(id, cancellationToken);
                        if (item == null) return null;
                        return new Story
                        {
                            Title = item.Title,
                            Uri = item.Url,
                            PostedBy = item.By,
                            Time = DateTimeOffset.FromUnixTimeSeconds(item.Time ?? 0).UtcDateTime,
                            Score = item.Score ?? 0,
                            CommentCount = item.Descendants ?? 0
                        };
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });
            });

            var results = await Task.WhenAll(tasks);
            return results.Where(r => r != null)!.Cast<Story>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching best stories (cached)");
            throw;
        }
    }
}
