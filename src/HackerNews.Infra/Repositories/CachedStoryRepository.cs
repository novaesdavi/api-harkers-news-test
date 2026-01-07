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

    public async Task<IEnumerable<Story>> BestStoriesWithNoParalelism(CancellationToken cancellationToken = default)
    {
        try
        {
            var stories = await _cache.GetOrCreateAsync("BestStories", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(300);
                var list = (await _inner.BestStoriesWithNoParalelism(cancellationToken)).ToList();
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
}
