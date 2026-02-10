namespace Application.Topics;

public interface ITopicsService
{
    Task<PaginatedList<TopicResponseDto>> GetTopicsAsync(
        int pageNumber = 1,
        int pageSize = 10, 
        bool includeDeleted = false, 
        CancellationToken ct = default
    );
    Task<TopicResponseDto> GetTopicAsync(Guid id, CancellationToken ct);
    Task<TopicResponseDto> CreateTopicAsync(CreateTopicDto dto, CancellationToken ct);
    Task<TopicResponseDto> UpdateTopicAsync(Guid id, UpdateTopicDto dto, CancellationToken ct);
    Task DeleteTopicAsync(Guid id, CancellationToken ct);
    Task<List<TopicResponseDto>> GetDeletedTopicsAsync(CancellationToken ct = default);
}
