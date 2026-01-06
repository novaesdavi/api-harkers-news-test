using HackerNews.Domain.Entities;
using HackerNews.Infra.Services;
using Serilog;

namespace HackerNews.Infra.Repositories;

public class HackerNewsStoryRepository : IStoryRepository
{
    private readonly IHackerNewsApi _api;
    private readonly ILogger _logger;

    public HackerNewsStoryRepository(IHackerNewsApi api, ILogger logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<IEnumerable<Story>> BestStories(CancellationToken cancellationToken = default)
    {
        try
        {
            var ids = await _api.GetBestStoriesIds();

            var semaphore = new SemaphoreSlim(10);
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
}
