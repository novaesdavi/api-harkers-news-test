using HackerNews.Domain.Entities;

namespace HackerNews.Infra.Repositories;

public interface IStoryRepository
{
    Task<IEnumerable<Story>> BestStoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Story>> BestStoriesWithNoParalelism(CancellationToken cancellationToken = default);
}