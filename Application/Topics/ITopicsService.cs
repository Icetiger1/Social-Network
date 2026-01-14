using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.ValueObjects;

namespace Application.Topics;

public interface ITopicsService
{
    Task<List<Topic>> GetTopicsAsync();
    Task<Topic> GetTopicAsync(Guid id);
    Task<Topic> CreateTopicAsync(Topic topicRequestDto);
    Task<Topic> UpdateTopicAsync(TopicId id, Topic topicRequestDto);
    Task DeleteTopicAsync(TopicId id);
}
