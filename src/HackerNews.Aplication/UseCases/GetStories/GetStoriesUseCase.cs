using HackerNews.Domain.Entities;
using HackerNews.Infra.Repositories;

namespace HackerNews.Aplication.UseCases.GetStories;

public class GetStoriesUseCase : IGetStoriesUseCase
{
    private readonly IStoryRepository _repository;

    public GetStoriesUseCase(IStoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Story>> HandleAsync(GetStoriesRequest request, CancellationToken cancellationToken = default)
    {
        var stories = await _repository.BestStoriesAsync(cancellationToken);
        return stories.OrderByDescending(s => s.Score).Take(request.N);
    }
}