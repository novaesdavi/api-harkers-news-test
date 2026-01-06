using HackerNews.Domain.Entities;

namespace HackerNews.Infra.Repositories;

public interface IStoryRepository
{
    Task<IEnumerable<Story>> BestStories(CancellationToken cancellationToken = default);
}