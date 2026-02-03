using Application.Common;
using Application.Data.DataBaseContext;
using Application.Dtos;
using Application.Extensions;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;

namespace Application.Topics;

public class TopicsService(IApplicationDbContext dbContext,
        ILogger<TopicsService> logger, 
        IDateTimeProvider dateTimeProvider) : ITopicsService
{
    /// <summary>
    /// Создание топика       
    /// </summary>
    /// <param name="topicRequestDto"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<TopicResponseDto> CreateTopicAsync(
        CreateTopicDto topicRequestDto, CancellationToken ct)
    {
        using var transaction = await dbContext.BeginTransactionAsync(ct);

        try
        {
            logger.LogDebug("Creating new topic with title: {Title}", topicRequestDto.Title);

            ValidateCreateRequest(topicRequestDto);

            Location location = Location.Of(
                topicRequestDto.Location.City,
                topicRequestDto.Location.Street);

            TopicId topicId = TopicId.New();

            Topic newTopic = Topic.Create(
                topicId,
                topicRequestDto.Title,
                topicRequestDto.EventStart,
                topicRequestDto.Summary,
                topicRequestDto.TopicType,
                location);

            await dbContext.Topics.AddAsync(newTopic, ct);
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            logger.LogInformation("Topic created successfully with ID: {Id}", topicId.Value);

            return newTopic.ToTopicResponseDto();
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
            logger.LogInformation("CreateTopicAsync operation was cancelled");
            throw;
        }
        catch (DomainException ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogWarning("Domain validation failed for topic creation: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Error occurred while creating topic");
            throw new ApplicationException("Failed to create topic", ex);
        }
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
        using var transaction = await dbContext.BeginTransactionAsync(ct);

        try
        {
            logger.LogDebug("Deleting topic with ID: {Id}", id);

            TopicId topicId = TopicId.Of(id);

            var topic = await dbContext.Topics
                .FirstOrDefaultAsync(t => t.Id == topicId, ct);

            if (topic == null)
            {
                logger.LogWarning("Topic with ID {Id} not found for deletion", id);
                throw new NotFoundException($"Topic with ID {id} not found");
            }

            topic.MarkAsDeleted();

            dbContext.Topics.Update(topic);
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            logger.LogInformation("Topic with ID {Id} marked as deleted", id);
        }
        catch (DomainException ex) when (ex.Message.Contains("TopicId не может быть пустым"))
        {
            await transaction.RollbackAsync(ct);
            logger.LogWarning("Invalid TopicId for deletion: {Id}", id);
            throw new NotFoundException($"Topic with ID {id} not found");
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
            logger.LogInformation("DeleteTopicAsync operation was cancelled for ID: {Id}", id);
            throw;
        }
        catch (NotFoundException)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Error occurred while deleting topic with ID: {Id}", id);
            throw new ApplicationException($"Failed to delete topic with ID {id}", ex);
        }
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
        try
        {
            TopicId topicId = TopicId.Of(id);
            var topic = await dbContext.Topics
                .Include(t => t.Location)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == topicId, ct);

            if (topic == null)
            {
                throw new TopicNotFoundException(id);
            }

            return topic.ToTopicResponseDto();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (TopicNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Failed to retrieve topic with ID {id}", ex);
        }
    }

    /// <summary>
    /// Получение нескольких топиков
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<PaginatedList<TopicResponseDto>> GetTopicsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        bool includeDeleted = false, 
        CancellationToken ct = default)
    {
        try
        {
            logger.LogInformation("Getting topics. Page: {Page}, Size: {Size}",
            pageNumber, pageSize);

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = dbContext.Topics
                .Include(t => t.Location)
                .Where(t => t.DeletedAt == null)
                .AsNoTracking();

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize) // Пропускаем предыдущие страницы
                .Take(pageSize)                    // Берем только нужное количество
                .Select(t => t.ToTopicResponseDto())          // Проецируем в DTO
                .ToListAsync(ct);

            logger.LogInformation("Retrieved {Count} topics out of {Total}, (includeDeleted: {IncludeDeleted})",
                items.Count, totalCount, includeDeleted);

            return new PaginatedList<TopicResponseDto>(items, totalCount, pageNumber, pageSize);
        }
        catch  (OperationCanceledException)
        {
            logger.LogInformation("GetTopicsAsync operation was cancelled");
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while getting all topics");
            throw new ApplicationException("Failed to retrieve topics", e);
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
        UpdateTopicDto dto, 
        CancellationToken ct)
    {
        using var transaction = await dbContext.BeginTransactionAsync(ct);

        try
        {
            logger.LogDebug("Updating topic with ID: {Id}", id);

            ValidateUpdateRequest(dto);

            TopicId topicId = TopicId.Of(id);

            var topic = await dbContext.Topics
                .Include(t => t.Location)
                .FirstAsync(t => t.Id == topicId, ct);

            if (topic is null)
            {
                logger.LogWarning("Topic with ID {Id} not found for update", id);
                throw new NotFoundException($"Topic with ID {id} not found");
            }

            topic.Update(
                dto.Title,
                dto.EventStart,
                dto.Summary,
                dto.TopicType,
                Location.Of(
                dto.Location.City,
                dto.Location.Street)
            );

            dbContext.Topics.Update(topic);
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            logger.LogInformation("Topic with ID {Id} updated successfully", id);
            return topic.ToTopicResponseDto();
        }
        catch (DomainException ex) when (ex.Message.Contains("TopicId не может быть пустым"))
        {
            await transaction.RollbackAsync(ct);
            logger.LogWarning("Invalid TopicId for update: {Id}", id);
            throw new NotFoundException($"Topic with ID {id} not found");
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
            logger.LogInformation("UpdateTopicAsync operation was cancelled for ID: {Id}", id);
            throw;
        }
        catch (NotFoundException)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
        catch (DomainException ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogWarning("Domain validation failed for topic update: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Error occurred while updating topic with ID: {Id}", id);
            throw new ApplicationException($"Failed to update topic with ID {id}", ex);
        }
    }

    /// <summary>
    /// получение удаленных топиков
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="ApplicationException"></exception>
    public async Task<List<TopicResponseDto>> GetDeletedTopicsAsync(CancellationToken ct)
    {
        try
        {
            logger.LogDebug("Getting deleted topics");

            var deletedTopics = await dbContext.Topics
                .Include(t => t.Location)
                .Where(t => t.DeletedAt != null) 
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync(ct);

            logger.LogInformation("Retrieved {Count} deleted topics", deletedTopics.Count);

            return deletedTopics.ToTopicResponseDtoList();
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("GetDeletedTopicsAsync operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting deleted topics");
            throw new ApplicationException("Failed to retrieve deleted topics", ex);
        }
    }


    /// <summary>
    /// Валидация запроса создания
    /// </summary>
    private static void ValidateCreateRequest(CreateTopicDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new DomainException("Title is required");

        if (string.IsNullOrWhiteSpace(request.Summary))
            throw new DomainException("Summary is required");

        if (string.IsNullOrWhiteSpace(request.TopicType))
            throw new DomainException("TopicType is required");

        if (request.Location == null)
            throw new DomainException("Location is required");

        if (string.IsNullOrWhiteSpace(request.Location.City))
            throw new DomainException("City is required");

        if (string.IsNullOrWhiteSpace(request.Location.Street))
            throw new DomainException("Street is required");
    }

    /// <summary>
    /// Валидация запроса обновления
    /// </summary>
    private static void ValidateUpdateRequest(UpdateTopicDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new DomainException("Title is required");

        if (string.IsNullOrWhiteSpace(request.Summary))
            throw new DomainException("Summary is required");

        if (string.IsNullOrWhiteSpace(request.TopicType))
            throw new DomainException("TopicType is required");

        if (request.Location == null)
            throw new DomainException("Location is required");

        if (string.IsNullOrWhiteSpace(request.Location.City))
            throw new DomainException("City is required");

        if (string.IsNullOrWhiteSpace(request.Location.Street))
            throw new DomainException("Street is required");
    }

}
