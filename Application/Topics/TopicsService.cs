using Application.Data.DataBaseContext;
using Application.Topics.Dtos;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Topics;

public class TopicsService(IApplicationDbContext dbContext,
        ILogger<TopicsService> logger) : ITopicsService
{
    /// <summary>
    /// Создание топика       
    /// </summary>
    /// <param name="topicRequestDto"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<TopicResponseDto> CreateTopicAsync(CreateTopicRequestDto topicRequestDto, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Удаление топика
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task DeleteTopicAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Получение 1 топика
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<TopicResponseDto> GetTopicAsync(
        Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Получение нескольких топиков
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<List<TopicResponseDto>> GetTopicsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        try
        {
            // Проверка входных параметров
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10; // Ограничиваем размер страницы

            logger.LogDebug("Getting topics. Page: {Page}, Size: {Size}, Search: {Search}",
                pageNumber, pageSize, searchTerm);

            //var topics = await dbContext.Topics
            //    .AsNoTracking()
            //    .ToListAsync(ct);

            // Начинаем запрос с базового IQueryable
            var query = dbContext.Topics
                .AsNoTracking(); // Только для чтения

            // Получаем общее количество для пагинации
            var totalCount = await query.CountAsync(ct);

            // Применяем пагинацию
            var topics = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => MapToDto(t)) // Проецируем сразу в DTO 
                .ToListAsync(ct);

            return topics;
        }
        catch (Exception)
        {
            return new List<TopicResponseDto>();
        }
    }

    /// <summary>
    /// обновление топиков
    /// </summary>
    /// <param name="id"></param>
    /// <param name="topicRequestDto"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<TopicResponseDto> UpdateTopicAsync(
        Guid id, 
        UpdateTopicRequestDto topicRequestDto, 
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    // Ручной маппинг
    private static TopicResponseDto MapToDto(Topic topic)
    {
        return new TopicResponseDto
        {
            Id = topic.Id.Value,
            Title = topic.Title,
            EventStart = topic.EventStart,
            Summary = topic.Summary,
            TopicType = topic.TopicType,
            Location = new LocationDto
            (
                topic.Location.City,
                topic.Location.Street
            )
        };
    }
}
