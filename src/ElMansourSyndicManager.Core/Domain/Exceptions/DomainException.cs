namespace ElMansourSyndicManager.Core.Domain.Exceptions;

/// <summary>
/// Base exception for domain-related errors
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when validation fails
/// </summary>
public class ValidationException : DomainException
{
    public Dictionary<string, string[]> Errors { get; }
    
    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }
    
    public ValidationException(string message, Dictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }
}

/// <summary>
/// Thrown when an entity is not found
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object id) 
        : base($"{entityName} with id '{id}' was not found.") { }
    
    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Thrown when an operation is not authorized
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message) : base(message) { }
}

/// <summary>
/// Thrown when a business rule is violated
/// </summary>
public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message) { }
}

/// <summary>
/// Thrown when a conflict occurs during sync
/// </summary>
public class SyncConflictException : DomainException
{
    public object LocalEntity { get; }
    public object CloudEntity { get; }
    
    public SyncConflictException(string message, object localEntity, object cloudEntity) 
        : base(message)
    {
        LocalEntity = localEntity;
        CloudEntity = cloudEntity;
    }
}

