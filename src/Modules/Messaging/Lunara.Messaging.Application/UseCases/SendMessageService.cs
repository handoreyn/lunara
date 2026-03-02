using System.Text.Json;
using Lunara.BuildingBlocks.Outbox;
using Lunara.Messaging.Application.DTOs;
using Lunara.Messaging.Application.Exceptions;
using Lunara.Messaging.Application.Ports;
using Lunara.Messaging.Domain.DomainEvents;
using Lunara.Messaging.Domain.Entities;

namespace Lunara.Messaging.Application.UseCases;

/// <summary>
/// Sends a message within the conversation that belongs to the given match,
/// creating the conversation on the first message when none exists yet.
/// </summary>
public sealed class SendMessageService(
    IMatchReadService matchReadService,
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository,
    IUnitOfWork unitOfWork,
    IClock clock,
    IOutboxWriter outboxWriter)
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
    ///   Thrown when the trimmed message text is empty or exceeds 2000 characters,
    ///   or the sender is not a participant of the conversation.
    /// </exception>
    public async Task<SendMessageResult> SendAsync(
        SendMessageRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        bool matchExists = await matchReadService
            .MatchExistsAsync(request.MatchId, ct)
            .ConfigureAwait(false);

        if (!matchExists)
        {
            throw new MatchNotFoundException(request.MatchId);
        }

        Conversation? conversation = await conversationRepository
            .GetByMatchIdAsync(request.MatchId, ct)
            .ConfigureAwait(false);

        bool isNewConversation = conversation is null;

        if (isNewConversation)
        {
            conversation = Conversation.CreateFromMatch(
                request.MatchId,
                request.SenderId,
                request.RecipientId,
                clock.UtcNow);
        }

        try
        {
            return await PersistMessageAsync(conversation!, request, addConversation: isNewConversation, ct)
                .ConfigureAwait(false);
        }
        catch (Exception) when (isNewConversation)
        {
            // A concurrent request won the race and inserted the conversation first.
            // Reload the existing conversation and retry without inserting.
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

        string payloadJson = JsonSerializer.Serialize(new
        {
            conversationId = evt.ConversationId.Value,
            messageId = evt.MessageId.Value,
            matchId = request.MatchId.Value,
            senderId = evt.SenderId.Value,
            recipientId = evt.RecipientId.Value,
            text = evt.Text,
            occurredAtUtc = evt.OccurredAtUtc,
        });

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
