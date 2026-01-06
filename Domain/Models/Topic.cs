namespace Domain.Models;

/// <summary>
/// Сущность темы 
/// </summary>
public class Topic : Entity<TopicId>
{
    public string Title { get; set; } = default!;
    public DateTime? EventStart { get; set; } = default!;
    public string Summary { get; set; } = default!;
    public string TopicType { get; set; } = default!;
    public Location Location { get; set; } = default!;

    /// <summary>
    /// Создание темы
    /// </summary>
    /// <param name="id">идентификатор темы</param>
    /// <param name="title">заголовок темы</param>
    /// <param name="eventStart">дата начала события</param>
    /// <param name="summary">краткое описание темы</param>
    /// <param name="topicType">тип темы</param>
    /// <param name="location">место проведения</param>
    /// <returns></returns>
    public static Topic Create(
        TopicId id, string title, DateTime? eventStart, 
        string summary, string topicType, Location location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        ArgumentException.ThrowIfNullOrWhiteSpace(topicType);

        Topic topic = new()
        {
            Id = id,
            Title = title,
            EventStart = eventStart,
            Summary = summary,
            TopicType = topicType,
            Location = location
        };

        return topic;
    }
}
