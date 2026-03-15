using Lunara.BuildingBlocks.Moderation;
using Lunara.Messaging.Application.DTOs;
using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Domain.Entities;

namespace Lunara.Messaging.Application.UseCases;

/// <summary>
/// Handles sending a message from one user to another.
/// The operation is rejected when a block exists in either direction.
/// </summary>
public sealed class SendMessageService(
    IMessageRepository messageRepository,
    IUnitOfWork unitOfWork,
    IClock clock,
    IBlockChecker blockChecker)
{
    /// <summary>
    /// Sends a message from <paramref name="request"/>.<c>SenderId</c> to
    /// <paramref name="request"/>.<c>RecipientId</c>.
    /// </summary>
    /// <param name="request">The message details.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="SendMessageResult"/> with the new message identifier.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when sender and recipient are the same user.</exception>
    /// <exception cref="InvalidOperationException">
    ///   Thrown when a block exists between the sender and recipient in either direction.
    /// </exception>
    public async Task<SendMessageResult> SendAsync(
        SendMessageRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        bool blocked = await blockChecker.IsBlockedInEitherDirectionAsync(
            request.SenderId.Value,
            request.RecipientId.Value,
            ct)
            .ConfigureAwait(false);

        if (blocked)
        {
            throw new InvalidOperationException(
                "Cannot send a message: a block exists between the sender and the recipient.");
        }

        Message message = Message.Create(
            request.SenderId,
            request.RecipientId,
            request.Body,
            clock.UtcNow);

        await messageRepository.AddAsync(message, ct).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(ct).ConfigureAwait(false);

        return new SendMessageResult(message.Id.Value);
    }
}
