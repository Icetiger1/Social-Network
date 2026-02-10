using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TopicsController(ITopicsService topicsService)
    : ControllerBase
{
    /// <summary>
    /// Получить все темы
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <param name="includeDeleted"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
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
        try
        {
            if (pageNumber < 1)
            {
                ModelState.AddModelError(nameof(pageNumber), "Номер страницы должен быть больше чем 0");
                return ValidationProblem(ModelState);
            }

            if (pageSize < 1 || pageSize > 100)
            {
                ModelState.AddModelError(nameof(pageSize), "Количество страниц должно быть от 1 до 100");
                return ValidationProblem(ModelState);
            }

            var result = await topicsService.GetTopicsAsync(pageNumber, pageSize, includeDeleted, ct);
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Ошибка получения топиков",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
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
    /// <param name="dto">Данные для создания темы</param>
    /// <param name="ct">Токен отмены</param>
    /// <returns>Созданная тема</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TopicResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TopicResponseDto>> CreateTopic(
        [FromBody] CreateTopicDto dto,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await topicsService.CreateTopicAsync(dto, ct);

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Ошибка создания топика",
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
        [FromBody] UpdateTopicDto dto,
        CancellationToken ct = default)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await topicsService.UpdateTopicAsync(id, dto, ct);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return Problem(
                title: "Ошибка обновления топика", 
                detail: ex.Message, 
                statusCode: StatusCodes.Status404NotFound 
            );
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Ошибка обновления топика",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Удалить тему (мягкое удаление)
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
            return Problem(
                title: "Ошибка удаления топика",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound
            );
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Ошибка удаления топика",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
