using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models;

/// <summary>
/// Тема
/// </summary>
public class Topic : Entity<TopicId>
{
    private string _title;
    private DateTime? _eventStart;
    private string _summary;
    private string _topicType;
    private Location _location;


    public string Title
    {
        get => _title;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Title cannot be empty");
            _title = value;
        }
    }

    public DateTime? EventStart
    {
        get => _eventStart;
        private set => _eventStart = value;
    }

    public string Summary
    {
        get => _summary;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Summary cannot be empty");
            _summary = value;
        }
    }

    public string TopicType
    {
        get => _topicType;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("TopicType cannot be empty");
            _topicType = value;
        }
    }

    public Location Location
    {
        get => _location;
        private set => _location = value ?? throw new ArgumentNullException(nameof(Location));
    }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }


    // Приватный конструктор для EF Core
    private Topic() { }

    /// <summary>
    /// Создание темы
    /// </summary>
    public static Topic Create(
        TopicId id, string title, DateTime? eventStart,
        string summary, string topicType, Location location)
    {
        var topic = new Topic
        {
            Id = id,
            Title = title,
            EventStart = eventStart,
            Summary = summary,
            TopicType = topicType,
            Location = location,
            CreatedAt = DateTime.UtcNow
        };

        return topic;
    }

    /// <summary>
    /// Метод для изменения состояния
    /// </summary>
    /// <param name="title"></param>
    /// <param name="eventStart"></param>
    /// <param name="summary"></param>
    /// <param name="topicType"></param>
    /// <param name="location"></param>
    public void Update(string title, DateTime? eventStart, string summary, string topicType, Location location)
    {
        Title = title;
        EventStart = eventStart;
        Summary = summary;
        TopicType = topicType;
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        DeletedAt = DateTime.UtcNow; 
    }
}
