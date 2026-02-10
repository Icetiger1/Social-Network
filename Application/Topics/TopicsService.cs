using Application.Common;
using Application.Data.DataBaseContext;
using Microsoft.Extensions.Logging;

namespace Application.Topics;

public class TopicsService(IApplicationDbContext dbContext,
        ILogger<TopicsService> logger, 
        IDateTimeProvider dateTimeProvider) : ITopicsService
{
    /// <summary>
    /// Создание топика      
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<TopicResponseDto> CreateTopicAsync(
        CreateTopicDto dto, CancellationToken ct)
    {
        using var transaction = await dbContext.BeginTransactionAsync(ct);

        try
        {
            ValidateTopicRequest(dto);

            Location location = Location.Of(
                dto.Location.City,
                dto.Location.Street);

            TopicId topicId = TopicId.New();

            Topic newTopic = Topic.Create(
                topicId,
                dto.Title,
                dto.EventStart,
                dto.Summary,
                dto.TopicType,
                location);

            await dbContext.Topics.AddAsync(newTopic, ct);
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return newTopic.ToTopicResponseDto();
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
        catch (DomainException)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
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
            TopicId topicId = TopicId.Of(id);

            var topic = await dbContext.Topics
                .FirstOrDefaultAsync(t => t.Id == topicId, ct);

            if (topic is null || topic.IsDeleted)
            {
                throw new TopicNotFoundException(id);
            }

            topic.MarkAsDeleted();

            dbContext.Topics.Update(topic);
            await dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DomainException ex) when (ex.Message.Contains("TopicId �� ����� ���� ������"))
        {
            await transaction.RollbackAsync(ct);
            throw new NotFoundException($"Topic with ID {id} not found");
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
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
            throw new ApplicationException($"Failed to delete topic with ID {id}", ex);
        }
    }

    /// <summary>
    /// Получить топик по id
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

            if (topic is null || topic.IsDeleted)
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
    /// Получение всех топиков
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
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = dbContext.Topics
                .Include(t => t.Location)
                .AsNoTracking();

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize) // ���������� ���������� ��������
                .Take(pageSize)                    // ����� ������ ������ ����������
                .Select(t => t.ToTopicResponseDto())          // ���������� � DTO
                .ToListAsync(ct);

            return new PaginatedList<TopicResponseDto>(items, totalCount, pageNumber, pageSize);
        }
        catch  (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new ApplicationException("Failed to retrieve topics", e);
        }
    }

    /// <summary>
    /// Обновление топиков
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
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
            ValidateTopicRequest(dto);

            TopicId topicId = TopicId.Of(id);

            var topic = await dbContext.Topics
                .Include(t => t.Location)
                .FirstOrDefaultAsync(t => t.Id == topicId, ct);

            if (topic is null || topic.IsDeleted)
            {
                throw new TopicNotFoundException(id);
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

            return topic.ToTopicResponseDto();
        }
        catch (DomainException ex) when (ex.Message.Contains("TopicId �� ����� ���� ������"))
        {
            await transaction.RollbackAsync(ct);
            throw new NotFoundException($"Topic with ID {id} not found");
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
        catch (NotFoundException)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
        catch (DomainException)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            throw new ApplicationException($"Failed to update topic with ID {id}", ex);
        }
    }

    /// <summary>
    /// Получение удаленных топиков
    /// </summary>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="ApplicationException"></exception>
    public async Task<List<TopicResponseDto>> GetDeletedTopicsAsync(CancellationToken ct)
    {
        try
        {
            var deletedTopics = await dbContext.Topics
                .Include(t => t.Location)
                .Where(t => t.IsDeleted) 
                .OrderByDescending(t => t.CreatedAt)
                .AsNoTracking()
                .ToListAsync(ct);

            return deletedTopics.ToTopicResponseDtoList();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to retrieve deleted topics", ex);
        }
    }

    /// <summary>
    /// Валидация топика
    /// </summary>
    private static void ValidateTopicRequest(ITopicRequestDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new DomainException("Title is required");

        if (string.IsNullOrWhiteSpace(dto.Summary))
            throw new DomainException("Summary is required");

        if (string.IsNullOrWhiteSpace(dto.TopicType))
            throw new DomainException("TopicType is required");

        if (dto.Location == null)
            throw new DomainException("Location is required");

        if (string.IsNullOrWhiteSpace(dto.Location.City))
            throw new DomainException("City is required");

        if (string.IsNullOrWhiteSpace(dto.Location.Street))
            throw new DomainException("Street is required");
    }

}
