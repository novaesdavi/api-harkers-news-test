using HackerNews.Domain.Entities;

namespace HackerNews.Aplication.UseCases.GetStories;

public interface IGetBestStoriesUseCase
{
    Task<IEnumerable<Story>> HandleAsync(GetStoriesRequest request, CancellationToken cancellationToken = default);
}