using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException() : base() { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }

    // Можно добавить дополнительные свойства
    public string? ResourceName { get; }
    public object? ResourceId { get; }

    public NotFoundException(string resourceName, object resourceId)
        : base($"Resource '{resourceName}' with identifier '{resourceId}' was not found.")
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }
}
