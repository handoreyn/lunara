using Lunara.Messaging.Application.DTOs;
using Lunara.Messaging.Application.UseCases;
using Lunara.Messaging.Domain.ValueObjects;
using Lunara.UnitTests.Messaging.Fakes;

namespace Lunara.UnitTests.Messaging;

/// <summary>Unit tests for <see cref="SendMessageService"/>.</summary>
public sealed class SendMessageServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static readonly UserId SenderId = new(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    private static readonly UserId RecipientId = new(new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

    private static (SendMessageService Svc, FakeMessageRepository Repo, FakeMessagingUnitOfWork Uow)
        Build(FakeMessagingBlockChecker? blockChecker = null)
    {
        FakeMessageRepository repo = new();
        FakeMessagingUnitOfWork uow = new();
        FakeMessagingClock clock = new(FixedNow);
        FakeMessagingBlockChecker checker = blockChecker ?? new FakeMessagingBlockChecker();
        SendMessageService svc = new(repo, uow, clock, checker);
        return (svc, repo, uow);
    }

    /// <summary>Sending a valid message persists it and saves once.</summary>
    [Fact]
    public async Task Send_ValidMessage_Persists_And_SavesOnce()
    {
        (SendMessageService svc, FakeMessageRepository repo, FakeMessagingUnitOfWork uow) = Build();

        SendMessageResult result = await svc.SendAsync(
            new SendMessageRequest(SenderId, RecipientId, "Hello!"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.MessageId);
        Assert.Single(repo.Messages);
        Assert.Equal(1, uow.SaveCallCount);
    }

    /// <summary>Sending a message to oneself throws <see cref="ArgumentException"/>.</summary>
    [Fact]
    public async Task Send_SameUser_Throws_ArgumentException()
    {
        (SendMessageService svc, _, _) = Build();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.SendAsync(
                new SendMessageRequest(SenderId, SenderId, "Hello!"),
                CancellationToken.None));
    }

    /// <summary>Sending a null request throws <see cref="ArgumentNullException"/>.</summary>
    [Fact]
    public async Task Send_NullRequest_Throws_ArgumentNullException()
    {
        (SendMessageService svc, _, _) = Build();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            svc.SendAsync(null!, CancellationToken.None));
    }

    /// <summary>
    /// Sending a message when the sender has blocked the recipient throws
    /// <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public async Task Send_SenderBlockedRecipient_Throws_InvalidOperationException()
    {
        FakeMessagingBlockChecker blockChecker = new();
        blockChecker.SeedBlock(SenderId.Value, RecipientId.Value);
        (SendMessageService svc, _, _) = Build(blockChecker);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.SendAsync(
                new SendMessageRequest(SenderId, RecipientId, "Hello!"),
                CancellationToken.None));
    }

    /// <summary>
    /// Sending a message when the recipient has blocked the sender throws
    /// <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public async Task Send_RecipientBlockedSender_Throws_InvalidOperationException()
    {
        FakeMessagingBlockChecker blockChecker = new();
        blockChecker.SeedBlock(RecipientId.Value, SenderId.Value);
        (SendMessageService svc, _, _) = Build(blockChecker);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.SendAsync(
                new SendMessageRequest(SenderId, RecipientId, "Hello!"),
                CancellationToken.None));
    }

    /// <summary>No message is persisted when a block violation is raised.</summary>
    [Fact]
    public async Task Send_BlockViolation_NoMessagePersisted()
    {
        FakeMessagingBlockChecker blockChecker = new();
        blockChecker.SeedBlock(SenderId.Value, RecipientId.Value);
        (SendMessageService svc, FakeMessageRepository repo, _) = Build(blockChecker);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.SendAsync(
                new SendMessageRequest(SenderId, RecipientId, "Hello!"),
                CancellationToken.None));

        Assert.Empty(repo.Messages);
    }
}
