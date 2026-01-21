using Application.Topics.Dtos;

namespace Application.Topics;

public interface ITopicsService
{
    Task<List<TopicResponseDto>> GetTopicsAsync(bool includeDeleted, CancellationToken ct = default);
    Task<TopicResponseDto> GetTopicAsync(Guid id, CancellationToken ct);
    Task<TopicResponseDto> CreateTopicAsync(CreateTopicRequestDto topicRequestDto, CancellationToken ct);
    Task<TopicResponseDto> UpdateTopicAsync(Guid id, UpdateTopicRequestDto topicRequestDto, CancellationToken ct);
    Task DeleteTopicAsync(Guid id, CancellationToken ct);
    Task<List<TopicResponseDto>> GetDeletedTopicsAsync(CancellationToken ct = default);
}
