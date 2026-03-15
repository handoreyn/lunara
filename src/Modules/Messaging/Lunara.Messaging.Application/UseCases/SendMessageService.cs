using Lunara.BuildingBlocks.EventContracts.V1;
using Lunara.BuildingBlocks.Moderation;
using Lunara.BuildingBlocks.Outbox;
using Lunara.Messaging.Application.DTOs;
using Lunara.Messaging.Application.Exceptions;
using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Domain.DomainEvents;
using Lunara.Messaging.Domain.Entities;
using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.Messaging.Application.UseCases;

/// <summary>
/// Sends a message within the conversation that belongs to the given match,
/// creating the conversation on the first message when none exists yet.
/// The operation is rejected when a block exists in either direction between the two participants.
/// </summary>
public sealed class SendMessageService(
    IMatchReadService matchReadService,
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository,
    IUnitOfWork unitOfWork,
    IClock clock,
    IOutboxWriter outboxWriter,
    IBlockChecker blockChecker)
{
    /// <summary>
    /// Sends a message and returns the identifiers of the affected conversation and message.
    /// </summary>
    /// <param name="request">The send-message request.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="SendMessageResult"/> containing the conversation and message ids.</returns>
    /// <exception cref="ArgumentNullException">
    ///   Thrown when <paramref name="request"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="MatchNotFoundException">
    ///   Thrown when the match referenced by <see cref="SendMessageRequest.MatchId"/> does not exist.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   Thrown when the sender is not a participant of the match, the trimmed message text
    ///   is empty, or the text exceeds 2000 characters.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   Thrown when a block exists between the sender and the other match participant in either direction.
    /// </exception>
    public async Task<SendMessageResult> SendAsync(
        SendMessageRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        MatchParticipants participants = await matchReadService
            .GetParticipantsAsync(request.MatchId, ct)
            .ConfigureAwait(false)
            ?? throw new MatchNotFoundException(request.MatchId);

        if (participants.User1Id != request.SenderId && participants.User2Id != request.SenderId)
        {
            throw new ArgumentException(
                "Sender is not a participant of this match.",
                nameof(request));
        }

        // Determine the recipient and reject if a block exists in either direction.
        UserId recipientId = participants.User1Id == request.SenderId
            ? participants.User2Id
            : participants.User1Id;

        bool blocked = await blockChecker.IsBlockedInEitherDirectionAsync(
            request.SenderId.Value,
            recipientId.Value,
            ct)
            .ConfigureAwait(false);

        if (blocked)
        {
            throw new InvalidOperationException(
                "Cannot send a message: a block exists between the sender and the recipient.");
        }

        Conversation? conversation = await conversationRepository
            .GetByMatchIdAsync(request.MatchId, ct)
            .ConfigureAwait(false);

        bool isNewConversation = conversation is null;

        if (isNewConversation)
        {
            conversation = Conversation.CreateFromMatch(
                request.MatchId,
                participants.User1Id,
                participants.User2Id,
                clock.UtcNow);
        }

        try
        {
            return await PersistMessageAsync(conversation!, request, addConversation: isNewConversation, ct)
                .ConfigureAwait(false);
        }
        catch (ConversationAlreadyExistsException) when (isNewConversation)
        {
            // A concurrent request won the race and inserted the conversation first.
            // The UnitOfWork translated the unique-constraint violation and cleared
            // the change tracker, so we can reload and retry without inserting.
            Conversation? existing = await conversationRepository
                .GetByMatchIdAsync(request.MatchId, ct)
                .ConfigureAwait(false);

            if (existing is null)
            {
                throw;
            }

            return await PersistMessageAsync(existing, request, addConversation: false, ct)
                .ConfigureAwait(false);
        }
    }

    private async Task<SendMessageResult> PersistMessageAsync(
        Conversation conversation,
        SendMessageRequest request,
        bool addConversation,
        CancellationToken ct)
    {
        (Message message, MessageSent evt) = conversation.SendMessage(
            request.SenderId,
            request.Text,
            clock.UtcNow);

        if (addConversation)
        {
            await conversationRepository
                .AddAsync(conversation, ct)
                .ConfigureAwait(false);
        }

        await messageRepository
            .AddAsync(message, ct)
            .ConfigureAwait(false);

        string payloadJson = EventJson.Serialize(new MessagingMessageSentV1(
            ConversationId: evt.ConversationId.Value,
            MessageId: evt.MessageId.Value,
            MatchId: request.MatchId.Value,
            SenderId: evt.SenderId.Value,
            RecipientId: evt.RecipientId.Value,
            Text: evt.Text,
            OccurredAtUtc: evt.OccurredAtUtc));

        await outboxWriter.EnqueueAsync(
            "messaging.message-sent.v1",
            payloadJson,
            evt.OccurredAtUtc,
            request.CorrelationId,
            ct)
            .ConfigureAwait(false);

        await unitOfWork
            .SaveChangesAsync(ct)
            .ConfigureAwait(false);

        return new SendMessageResult(
            ConversationId: evt.ConversationId.Value,
            MessageId: evt.MessageId.Value);
    }
}
