using Refit;
using HackerNews.Infra.Services.Dtos;

namespace HackerNews.Infra.Services;

public interface IHackerNewsApi
{
    [Get("/v0/beststories.json")]
    Task<int[]> GetBestStoriesIds();

    [Get("/v0/item/{id}.json")]
    Task<StoryDto?> GetItem(int id, CancellationToken cancellationToken = default);
}