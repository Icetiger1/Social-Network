using Application.Common;
using Application.Data.DataBaseContext;
using Application.Topics.Dtos;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

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
        CreateTopicRequestDto topicRequestDto, CancellationToken ct)
    {
        using var transaction = await dbContext.BeginTransactionAsync(ct);

        try
        {
            logger.LogDebug("Creating new topic with title: {Title}", topicRequestDto.Title);

            // Валидация входных данных
            ValidateCreateRequest(topicRequestDto);

            // Создаем Location из DTO
            var location = Location.Of(
                topicRequestDto.Location.City,
                topicRequestDto.Location.Street);

            // Создаем TopicId (нужно добавить метод New() в TopicId или генерировать Guid здесь)
            var topicId = TopicId.Of(Guid.NewGuid()); // Или TopicId.New() если добавите метод

            // Создаем доменную сущность
            var topic = Topic.Create(
                topicId,
                topicRequestDto.Title,
                topicRequestDto.EventStart,
                topicRequestDto.Summary,
                topicRequestDto.TopicType,
                location);

            // Устанавливаем метаданные создания
            topic.SetCreationMetadata(dateTimeProvider.UtcNow);

            // Добавляем в контекст
            await dbContext.Topics.AddAsync(topic, ct);
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            logger.LogInformation("Topic created successfully with ID: {Id}", topicId.Value);

            // Получаем созданную тему с включенным Location для возврата
            var createdTopic = await dbContext.Topics
                .Include(t => t.Location)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == topicId, ct);

            return MapToDto(createdTopic!);
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

            // Создаем TopicId из Guid
            var topicId = TopicId.Of(id);

            // Получаем сущность для удаления
            var topic = await dbContext.Topics
                .FirstOrDefaultAsync(t => t.Id == topicId && !t.IsDeleted, ct);

            if (topic == null)
            {
                logger.LogWarning("Topic with ID {Id} not found for deletion", id);
                throw new NotFoundException($"Topic with ID {id} not found");
            }

            // Мягкое удаление
            topic.MarkAsDeleted(dateTimeProvider.UtcNow);

            // Помечаем как измененную
            dbContext.Topics.Update(topic);

            // Сохраняем изменения
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
            logger.LogDebug("Getting topic with ID: {Id}", id);

            var topicId = TopicId.Of(id);
            var topic = await dbContext.Topics
                .Include(t => t.Location)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == topicId, ct);

            if (topic == null)
            {
                logger.LogWarning("Topic with ID {Id} not found", id);
                throw new NotFoundException($"Topic with ID {id} not found");
            }

            logger.LogDebug("Topic with ID {Id} found", id);
            return MapToDto(topic);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("GetTopicAsync operation was cancelled for ID: {Id}", id);
            throw;
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while getting topic with ID: {Id}", id);
            throw new ApplicationException($"Failed to retrieve topic with ID {id}", ex);
        }
    }

    /// <summary>
    /// Получение нескольких топиков
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<List<TopicResponseDto>> GetTopicsAsync(
        CancellationToken ct = default)
    {
        try
        {
            logger.LogDebug("Getting all topics");

            var topics = await dbContext.Topics
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync(ct);

            return topics.Select(MapToDto).ToList();
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
        UpdateTopicRequestDto topicRequestDto, 
        CancellationToken ct)
    {
        using var transaction = await dbContext.BeginTransactionAsync(ct);

        try
        {
            logger.LogDebug("Updating topic with ID: {Id}", id);

            // Валидация входных данных
            ValidateUpdateRequest(topicRequestDto);

            // Создаем TopicId из Guid
            var topicId = TopicId.Of(id);

            // Получаем сущность для изменения
            var topic = await dbContext.Topics
                .Include(t => t.Location)
                .FirstOrDefaultAsync(t => t.Id == topicId && !t.IsDeleted, ct);

            if (topic == null)
            {
                logger.LogWarning("Topic with ID {Id} not found for update", id);
                throw new NotFoundException($"Topic with ID {id} not found");
            }

            // Создаем новый Location из DTO
            var location = Location.Of(
                topicRequestDto.Location.City,
                topicRequestDto.Location.Street);

            // Обновляем тему через доменный метод
            topic.Update(
                topicRequestDto.Title,
                topicRequestDto.EventStart,
                topicRequestDto.Summary,
                topicRequestDto.TopicType,
                location);

            // Устанавливаем метаданные обновления
            topic.SetUpdateMetadata(dateTimeProvider.UtcNow);

            // Помечаем сущность как измененную
            dbContext.Topics.Update(topic);

            // Сохраняем изменения
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            logger.LogInformation("Topic with ID {Id} updated successfully", id);
            return MapToDto(topic);
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

    // Ручной маппинг топика
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

    /// <summary>
    /// Валидация запроса создания
    /// </summary>
    private static void ValidateCreateRequest(CreateTopicRequestDto request)
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
    private static void ValidateUpdateRequest(UpdateTopicRequestDto request)
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
