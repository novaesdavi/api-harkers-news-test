using HackerNews.Aplication.UseCases.GetStories;
using HackerNews.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace HackerNews.Api.Controllers;

[ApiController]
[Route("v0/[controller]")]
public class StoryController : ControllerBase
{
    private readonly IGetStoriesUseCase _storiesUseCase;
    private readonly IGetBestTopStoriesUseCase _topStoriesUseCase;

    public StoryController(IGetStoriesUseCase storiesUseCase, IGetBestTopStoriesUseCase topStoriesUseCase)
    {
        _storiesUseCase = storiesUseCase;
        _topStoriesUseCase = topStoriesUseCase;
    }


    [HttpGet("beststories")]
    public async Task<IActionResult> GetBestTopStories([FromQuery] int n = 0, CancellationToken cancellationToken = default)
    {
        if (n <= 0)
        {
           var storiesTodas = await _storiesUseCase.HandleAsync(cancellationToken);
            return Ok(storiesTodas);
        }
        var stories = await _topStoriesUseCase.HandleAsync(new GetStoriesRequest(n), cancellationToken);
        return Ok(stories);
    }
}