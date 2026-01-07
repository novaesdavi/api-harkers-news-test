using HackerNews.Domain.Entities;
using HackerNews.Infra.Repositories;

namespace HackerNews.Aplication.UseCases.GetStories;

public class GetBestStoriesUseCase : IGetStoriesUseCase
{
    private readonly IStoryRepository _repository;

    public GetBestStoriesUseCase(IStoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Story>> HandleAsync(CancellationToken cancellationToken = default)
    {
        //var stories = await _repository.BestStoriesWithNoParalelism(cancellationToken);

        var stories = await _repository.BestStoriesAsync(cancellationToken);

        return stories;
    }
}