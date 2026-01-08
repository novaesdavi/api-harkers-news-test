using HackerNews.Domain.Entities;

namespace HackerNews.Infra.Repositories;

public interface IStoryRepository
{
    Task<IEnumerable<Story>> BestStoriesAsync(CancellationToken cancellationToken = default);
    Task<Story> BestStoriesAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<int>> GetBestStoriesIdsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Story>> BestStoriesAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
}