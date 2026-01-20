using Application.Topics;
using Application.Topics.Dtos;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TopicsController(ITopicsService topicsService)
    : ControllerBase
{

    [HttpGet]
    public async Task<ActionResult<List<TopicResponseDto>>> GetTopics(
        CancellationToken ct
    )
    {
        return Ok(await topicsService.GetTopicsAsync(ct));
    }


}
