using Application.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Extensions;

public static class TopicExtensions
{
    /// <summary>
    /// Преобразование топика в TopicResponseDto
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    public static TopicResponseDto ToTopicResponseDto(this Topic topic)
    {
        return new TopicResponseDto(
            Id: topic.Id.Value,
            Title: topic.Title,
            Summary: topic.Summary,
            TopicType: topic.TopicType,
            City: topic.Location.City,
            Street: topic.Location.Street,
            EventStart: topic.EventStart,
            CreatedAt: topic.CreatedAt,
            UpdatedAt: topic.UpdatedAt,
            DeletedAt: topic.DeletedAt
        );
    }

    /// <summary>
    /// Преобразование списка топиков в TopicResponseDto
    /// </summary>
    /// <param name="topics"></param>
    /// <returns></returns>
    public static List<TopicResponseDto> ToTopicResponseDtoList(
        this List<Topic> topics)
    {
        return topics
            .Select(t => t.ToTopicResponseDto())
            .ToList();
    }
}
