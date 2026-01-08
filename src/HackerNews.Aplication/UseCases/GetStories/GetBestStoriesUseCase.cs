using HackerNews.Domain.Entities;
using HackerNews.Infra.Repositories;

namespace HackerNews.Aplication.UseCases.GetStories;

public class GetBestStoriesUseCase : IGetBestStoriesUseCase
{
    private readonly IStoryRepository _repository;

    public GetBestStoriesUseCase(IStoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Story>> HandleAsync(GetStoriesRequest request, CancellationToken cancellationToken = default)
    {

        if (request.N <= 0)
        {
            var stories = await _repository.BestStoriesAsync(cancellationToken);
            return stories.OrderByDescending(s => s.Score).Take(request.N);
        }
        else if (request.N >= 1)
        {
            var storiesId = await _repository.GetBestStoriesIdsAsync(cancellationToken);

            var idsToFetch = storiesId.Take(request.N).ToArray();

            var stories = await _repository.BestStoriesAsync(idsToFetch, cancellationToken);
            return stories.OrderByDescending(s => s.Score).Take(request.N);

        }

        return Enumerable.Empty<Story>();
    }
}