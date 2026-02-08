namespace Domain.Abstractions;

/// <summary>
/// Базовый класс для всех сущностей в доменной модели.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class Entity<T> : IEntity<T>
{
    public required T Id { get; set; }

    public bool IsDeleted { get; set; } = default!;
    public DateTimeOffset? DeletedAt { get; set; } = default!;
    public DateTimeOffset? UpdatedAt { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; } = default!;
}
