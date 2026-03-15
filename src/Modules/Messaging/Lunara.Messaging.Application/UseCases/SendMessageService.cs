using Lunara.BuildingBlocks.Moderation;
using Lunara.Messaging.Application.DTOs;

namespace Lunara.Messaging.Application.UseCases;

/// <summary>
/// Validates and processes a request to send a message from one user to another.
/// <para>
/// The send is rejected when a block exists in either direction between sender and recipient.
/// </para>
/// </summary>
public sealed class SendMessageService(IBlockChecker blockChecker)
{
    /// <summary>
    /// Processes a message-send request and returns the outcome.
    /// </summary>
    /// <param name="request">The send-message details.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>
    ///   A <see cref="SendMessageResult"/> with <c>Blocked = true</c> when a block is in effect,
    ///   or <c>Sent = true</c> when the message is accepted.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <c>null</c>.</exception>
    public async Task<SendMessageResult> SendAsync(SendMessageRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        bool blocked = await blockChecker.IsBlockedInEitherDirectionAsync(
            request.SenderId.Value,
            request.RecipientId.Value,
            ct)
            .ConfigureAwait(false);

        if (blocked)
        {
            return new SendMessageResult(Sent: false, Blocked: true);
        }

        // Message delivery logic would be implemented here.
        return new SendMessageResult(Sent: true, Blocked: false);
    }
}
