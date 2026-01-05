namespace Domain.Abstractions;

/// <summary>
/// Базовый интерфейс для всех сущностей в доменной модели.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IEntity<T> : IEntity
{
    public T id { get; set; }
}

public interface IEntity
{
    
}