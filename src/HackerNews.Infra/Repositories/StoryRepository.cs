using HackerNews.Domain.Entities;
using HackerNews.Infra.Services;
using Polly;
using System.Collections.Concurrent;
using Serilog;

namespace HackerNews.Infra.Repositories;

public class StoryRepository : IStoryRepository
{
    private readonly IHackerNewsApi _api;
    private readonly ILogger _logger;

    public StoryRepository(IHackerNewsApi api, ILogger logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<IEnumerable<int>> GetBestStoriesIdsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var ids = await _api.GetBestStoriesIds();
            return ids ?? Enumerable.Empty<int>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching best story ids");
            throw;
        }
    }

    public async Task<Story> BestStoriesAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
                var item = await _api.GetItem(id, cancellationToken);
                var result = new Story
                {
                    Title = item.Title,
                    Uri = item.Url,
                    PostedBy = item.By,
                    Time = DateTimeOffset.FromUnixTimeSeconds(item.Time ?? 0).UtcDateTime,
                    Score = item.Score ?? 0,
                    CommentCount = item.Descendants ?? 0
                };
            
            return result;

        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching best stories");
            throw;
        }
    }
    public async Task<IEnumerable<Story>> BestStoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {            
            var ids = await _api.GetBestStoriesIds();

            var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2);
            var tasks = ids.Select(async id =>
            {
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

            var results = await Task.WhenAll(tasks);
            return results.Where(r => r != null)!.Cast<Story>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching best stories");
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

            var results = await Task.WhenAll(tasks);
            return results.Where(r => r != null)!.Cast<Story>();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching best stories by id");
            throw;
        }
    }
}
