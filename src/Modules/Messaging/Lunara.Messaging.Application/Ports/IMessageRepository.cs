using Lunara.Messaging.Domain.Entities;

namespace Lunara.Messaging.Application.Ports;

/// <summary>Persistence port for <see cref="Message"/> entities.</summary>
public interface IMessageRepository
{
    /// <summary>Persists a newly created <see cref="Message"/>.</summary>
    /// <param name="message">The message to add.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    Task AddAsync(Message message, CancellationToken ct);
}
