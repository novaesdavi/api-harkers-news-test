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

    public async Task<Story> BestStoriesWithNoParalelismAsync(int id, CancellationToken cancellationToken = default)
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


    public async Task<IEnumerable<Story>> BestStoriesWithNoParalelismAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var ids = await _api.GetBestStoriesIds();
            _logger.Information($"IdsCount: {ids?.Count()}");

            List<Story> results = new List<Story>();
            foreach (var id in ids)
            {
                var item = await _api.GetItem(id, cancellationToken);
                results.Add(new Story
                {
                    Title = item.Title,
                    Uri = item.Url,
                    PostedBy = item.By,
                    Time = DateTimeOffset.FromUnixTimeSeconds(item.Time ?? 0).UtcDateTime,
                    Score = item.Score ?? 0,
                    CommentCount = item.Descendants ?? 0
                });
            }
            return results;

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
            //_logger.Information($"IdsCount: {ids?.Count()}");

            //SemaphoreSlim to limit concurrency works like Parallel.ForEachAsync
            //Just keeping the old code for reference

            //_logger.Information($"Processing with MaxDegreeOfParallelism: {Environment.ProcessorCount * 2}");

            //var options = new ParallelOptions
            //{
            //    MaxDegreeOfParallelism = Environment.ProcessorCount * 2
            //};
            //if (ids == null) return Enumerable.Empty<Story>();

            //var resultsBag = new ConcurrentBag<Story>();

            //await Parallel.ForEachAsync(ids, options, async (id, ct) =>
            //{
            //    var item = await _api.GetItem(id, ct);
            //    if (item == null) return;
            //    resultsBag.Add(new Story
            //    {
            //        Title = item.Title,
            //        Uri = item.Url,
            //        PostedBy = item.By,
            //        Time = DateTimeOffset.FromUnixTimeSeconds(item.Time ?? 0).UtcDateTime,
            //        Score = item.Score ?? 0,
            //        CommentCount = item.Descendants ?? 0
            //    });
            //});

            //return resultsBag.ToList();


            var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 4);
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


    public async Task<IEnumerable<Story>> BestStoriesByIdAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        try
        {
            if (ids == null) return Enumerable.Empty<Story>();

            var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 4);

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
