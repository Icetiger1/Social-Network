using Application.Dtos;
using Application.Topics;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TopicsController(ITopicsService topicsService)
    : ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<TopicResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<TopicResponseDto>>> GetTopics(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeDeleted = false,
        CancellationToken ct = default)
    {
        // Валидация параметров
        if (pageNumber < 1)
        {
            ModelState.AddModelError(nameof(pageNumber), "Page number must be greater than 0");
            return ValidationProblem(ModelState);
        }

        if (pageSize < 1 || pageSize > 100)
        {
            ModelState.AddModelError(nameof(pageSize), "Page size must be between 1 and 100");
            return ValidationProblem(ModelState);
        }

        var result = await topicsService.GetTopicsAsync(pageNumber, pageSize, includeDeleted, ct);
        return Ok(result);
    }

    /// <summary>
    /// Получить тему по ID
    /// </summary>
    /// <param name="id">Идентификатор темы</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Тема</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TopicResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TopicResponseDto>> GetTopic(
        Guid id,
        CancellationToken ct = default)
    {
        var result = await topicsService.GetTopicAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Создать новую тему
    /// </summary>
    /// <param name="request">Данные для создания темы</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Созданная тема</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TopicResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TopicResponseDto>> CreateTopic(
        [FromBody] CreateTopicRequestDto request,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await topicsService.CreateTopicAsync(request, ct);

            return CreatedAtAction(
                nameof(GetTopic),
                new { id = result.Id },
                result);
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Failed to create topic",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Обновить существующую тему
    /// </summary>
    /// <param name="id">Идентификатор темы</param>
    /// <param name="request">Данные для обновления</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Обновленная тема</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TopicResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TopicResponseDto>> UpdateTopic(
        Guid id,
        [FromBody] UpdateTopicRequestDto request,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await topicsService.UpdateTopicAsync(id, request, ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Failed to update topic",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Удалить тему
    /// </summary>
    /// <param name="id">Идентификатор темы</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>No Content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTopic(
        Guid id,
        CancellationToken ct = default)
    {
        try
        {
            await topicsService.DeleteTopicAsync(id, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Failed to delete topic",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
