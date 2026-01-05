namespace Domain.Exceptions;

/// <summary>
/// Исключение, связанное с доменной логикой.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base($"Domain exception: ({message}).")
    {
        
    }
}
