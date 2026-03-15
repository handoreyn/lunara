using Lunara.Messaging.Domain.Entities;

namespace Lunara.Messaging.Application.Ports;

/// <summary>Persistence port for storing messages.</summary>
public interface IMessageRepository
{
    /// <summary>Persists a new message.</summary>
    Task AddAsync(Message message, CancellationToken ct);
}
