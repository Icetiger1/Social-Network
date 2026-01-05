using Domain.Exceptions;

namespace Domain.ValueObjects;

/// <summary>
/// Представляет идентификатор темы.
/// </summary>
public record TopicId
{
    public Guid Value { get; }

    private TopicId(Guid value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Фабричный метод для создания экземпляра TopicId.
    /// </summary>
    /// <param name="value">Идентификатор темы</param>
    /// <returns></returns>
    /// <exception cref="DomainException"></exception>
    public static TopicId Of(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainException("TopicId не может быть пустым.");
        }
        return new TopicId(value);
    }
}