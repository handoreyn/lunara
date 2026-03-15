using Lunara.Messaging.Application.DTOs;
using Lunara.Messaging.Application.UseCases;
using Lunara.Messaging.Domain.ValueObjects;
using Lunara.UnitTests.Messaging.Fakes;

namespace Lunara.UnitTests.Messaging;

/// <summary>Unit tests for block-enforcement rules in <see cref="SendMessageService"/>.</summary>
public sealed class MessagingBlockEnforcementTests
{
    private static readonly UserId SenderId = new(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    private static readonly UserId RecipientId = new(new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

    /// <summary>Sending a message is rejected when a block exists in either direction.</summary>
    [Fact]
    public async Task Send_WhenBlocked_ReturnsBlockedResult_NotSent()
    {
        FakeMessagingBlockChecker blockChecker = new();
        blockChecker.SetBlocked(true);
        SendMessageService svc = new(blockChecker);

        SendMessageResult result = await svc.SendAsync(
            new SendMessageRequest(SenderId, RecipientId, "Hello"),
            CancellationToken.None);

        Assert.False(result.Sent);
        Assert.True(result.Blocked);
    }

    /// <summary>Sending a message succeeds when no block exists between the users.</summary>
    [Fact]
    public async Task Send_WhenNotBlocked_ReturnsSentResult()
    {
        FakeMessagingBlockChecker blockChecker = new();
        blockChecker.SetBlocked(false);
        SendMessageService svc = new(blockChecker);

        SendMessageResult result = await svc.SendAsync(
            new SendMessageRequest(SenderId, RecipientId, "Hello"),
            CancellationToken.None);

        Assert.True(result.Sent);
        Assert.False(result.Blocked);
    }
}
