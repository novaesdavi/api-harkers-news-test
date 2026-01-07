using HackerNews.Aplication.UseCases.GetStories;
using HackerNews.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace HackerNews.Api.Controllers;

[ApiController]
[Route("v0/[controller]")]
public class StoryController : ControllerBase
{
    private readonly IGetStoriesUseCase _useCase;

    public StoryController(IGetStoriesUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet("beststories")]
    public async Task<IActionResult> GetBestStories([FromQuery] int n = 10, CancellationToken cancellationToken = default)
    {
        if (n <= 0) return BadRequest("n must be greater than 0");
        var stories = await _useCase.HandleAsync(new GetStoriesRequest(n), cancellationToken);
        return Ok(stories);
    }
}