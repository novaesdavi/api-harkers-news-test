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

    public async Task<IEnumerable<int>> GetBestStoriesIdsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var ids = await _cache.GetOrCreateAsync("BestStoriesIds", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(300);
                return (await _inner.GetBestStoriesIdsAsync(cancellationToken)).ToArray();
            });

            return ids;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching best stories ids (cached)");
            throw;
        }
    }

    public async Task<IEnumerable<Story>> BestStoriesAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            if (ids == null) return Enumerable.Empty<Story>();

            var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2);
            var tasks = ids.Select(async id =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await BestStoriesAsync(id, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
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

    public async Task<IEnumerable<Story>> BestStoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stories = await _cache.GetOrCreateAsync("BestStories", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(300);
                var list = (await _inner.BestStoriesAsync(cancellationToken)).ToList();
                return list;
            });

            return stories;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching best stories (cached)");
            throw;
        }
    }

    public async Task<Story> BestStoriesAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var story = await _cache.GetOrCreateAsync($"story_id_{id}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(300);
                return (await _inner.BestStoriesAsync(id, cancellationToken));

            });
            return story;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching best stories (cached)");
            throw;
        }
    }
}
