using Application.Dtos;
using Shared;

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
    Task<TopicResponseDto> CreateTopicAsync(CreateTopicRequestDto topicRequestDto, CancellationToken ct);
    Task<TopicResponseDto> UpdateTopicAsync(Guid id, UpdateTopicRequestDto topicRequestDto, CancellationToken ct);
    Task DeleteTopicAsync(Guid id, CancellationToken ct);
    Task<List<TopicResponseDto>> GetDeletedTopicsAsync(CancellationToken ct = default);
}
