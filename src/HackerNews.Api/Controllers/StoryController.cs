using HackerNews.Aplication.UseCases.GetStories;
using HackerNews.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace HackerNews.Api.Controllers;

[ApiController]
[Route("v0/[controller]")]
public class StoryController : ControllerBase
{
    private readonly IGetBestStoriesUseCase _storiesUseCase;


    public StoryController(IGetBestStoriesUseCase storiesUseCase)
    {
        _storiesUseCase = storiesUseCase;
    }


    [HttpGet("beststories")]
    public async Task<IActionResult> GetBestTopStories([FromQuery] int n = 0, CancellationToken cancellationToken = default)
    {
           var stories = await _storiesUseCase.HandleAsync(new GetStoriesRequest(n), cancellationToken);
            return Ok(stories);

    }
}