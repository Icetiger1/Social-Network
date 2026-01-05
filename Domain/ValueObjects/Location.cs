namespace Domain.ValueObjects;

/// <summary>
/// Представляет местоположение с городом и улицей.
/// </summary>
public record Location
{
    public string City { get; set;} = default!;
    public string Street { get; set;} = default!;

    private Location(string city, string street)
    {
        City = city;
        Street = street;
    }

    /// <summary>
    /// Фабричный метод для создания экземпляра Location.
    /// </summary>
    /// <param name="city">город</param>
    /// <param name="street"></param>
    /// <returns></returns>
    public static Location Of(string city, string street)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(street);

        return new Location(city, street);
    }
}