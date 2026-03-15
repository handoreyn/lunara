namespace Lunara.Moderation.Application.Exceptions;

/// <summary>
/// Thrown by the persistence layer when an attempt to insert an entity would violate a unique constraint.
/// <para>
/// Consumers may catch this exception to treat the operation as an idempotent no-op rather than
/// surfacing a failure to the caller.
/// </para>
/// </summary>
public sealed class DuplicateEntityException : Exception
{
    /// <summary>Initializes a new <see cref="DuplicateEntityException"/> with a default message.</summary>
    public DuplicateEntityException()
        : base("A duplicate record was detected.")
    {
    }

    /// <summary>Initializes a new <see cref="DuplicateEntityException"/> with the supplied message.</summary>
    public DuplicateEntityException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="DuplicateEntityException"/> with the supplied message
    /// and inner exception.
    /// </summary>
    public DuplicateEntityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
