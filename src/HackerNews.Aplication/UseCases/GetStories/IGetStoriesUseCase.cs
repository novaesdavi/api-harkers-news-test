using HackerNews.Domain.Entities;

namespace HackerNews.Aplication.UseCases.GetStories;

public interface IGetStoriesUseCase
{
    Task<IEnumerable<Story>> HandleAsync(GetStoriesRequest request, CancellationToken cancellationToken = default);
}