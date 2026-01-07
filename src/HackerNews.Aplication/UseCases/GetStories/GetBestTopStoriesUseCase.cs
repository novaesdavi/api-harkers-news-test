using HackerNews.Domain.Entities;
using HackerNews.Infra.Repositories;
using System.Linq;

namespace HackerNews.Aplication.UseCases.GetStories;

public class GetBestTopStoriesUseCase : IGetBestTopStoriesUseCase
{
    private readonly IStoryRepository _repository;

    public GetBestTopStoriesUseCase (IStoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Story>> HandleAsync(GetStoriesRequest request, CancellationToken cancellationToken = default)
    {

        var storiesId = await _repository.GetBestStoriesIdsAsync(cancellationToken);

        var idsToFetch = storiesId.Take(request.N).ToArray();

        var stories = await _repository.BestStoriesByIdAsync(idsToFetch, cancellationToken);

        return stories.OrderByDescending(s => s.Score).Take(request.N);
    }
}