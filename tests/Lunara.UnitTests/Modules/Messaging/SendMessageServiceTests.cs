using Lunara.Messaging.Application.DTOs;
using Lunara.Messaging.Application.Exceptions;
using Lunara.Messaging.Application.UseCases;
using Lunara.Messaging.Domain.Entities;
using Lunara.Messaging.Domain.ValueObjects;
using Lunara.UnitTests.Modules.Messaging.Fakes;

namespace Lunara.UnitTests.Modules.Messaging;

/// <summary>Unit tests for <see cref="SendMessageService"/>.</summary>
public sealed class SendMessageServiceTests
{
    // ── Fixtures ────────────────────────────────────────────────────────────

    private static readonly DateTimeOffset FixedNow =
        new(2026, 3, 2, 12, 0, 0, TimeSpan.Zero);

    // rawA < rawB so canonical ordering is: User1 = SenderA, User2 = SenderB
    private static readonly UserId SenderA = new(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
    private static readonly UserId SenderB = new(new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
    private static readonly MatchId AMatchId = new(new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));

    // ── Builder ─────────────────────────────────────────────────────────────

    private sealed record Fixtures(
        SendMessageService Svc,
        FakeMatchReadService MatchSvc,
        FakeConversationRepository ConvRepo,
        FakeMessageRepository MsgRepo,
        FakeUnitOfWork Uow,
        FakeOutboxWriter Outbox);

    private static Fixtures Build(
        bool matchExists = true,
        Conversation? existingConversation = null)
    {
        FakeMatchReadService matchSvc = new();
        if (matchExists)
        {
            matchSvc.SeedMatch(AMatchId, SenderA, SenderB);
        }

        FakeConversationRepository convRepo = new();
        if (existingConversation is not null)
        {
            convRepo.Seed(existingConversation);
        }

        FakeMessageRepository msgRepo = new();
        FakeUnitOfWork uow = new();
        FakeClock clock = new(FixedNow);
        FakeOutboxWriter outbox = new();

        SendMessageService svc = new(matchSvc, convRepo, msgRepo, uow, clock, outbox);
        return new Fixtures(svc, matchSvc, convRepo, msgRepo, uow, outbox);
    }

    private static SendMessageRequest DefaultRequest(
        UserId? sender = null,
        string text = "Hello!")
    {
        return new SendMessageRequest(
            MatchId: AMatchId,
            SenderId: sender ?? SenderA,
            Text: text,
            CorrelationId: "corr-123");
    }

    // ── Match does not exist ─────────────────────────────────────────────────

    [Fact]
    public async Task SendAsync_WhenMatchDoesNotExist_ThrowsMatchNotFoundException()
    {
        Fixtures f = Build(matchExists: false);

        await Assert.ThrowsAsync<MatchNotFoundException>(
            () => f.Svc.SendAsync(DefaultRequest(), CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_WhenMatchDoesNotExist_NoRepoCallsAndNoOutboxEnqueue()
    {
        Fixtures f = Build(matchExists: false);

        try
        {
            await f.Svc.SendAsync(DefaultRequest(), CancellationToken.None);
        }
        catch (MatchNotFoundException)
        {
            // expected
        }

        Assert.Equal(0, f.ConvRepo.AddCallCount);
        Assert.Equal(0, f.MsgRepo.AddCallCount);
        Assert.Equal(0, f.Outbox.EnqueueCallCount);
        Assert.Equal(0, f.Uow.SaveCallCount);
    }

    // ── Conversation does not exist yet ──────────────────────────────────────

    [Fact]
    public async Task SendAsync_WhenNoConversation_CreatesConversationAndAddsMessage()
    {
        Fixtures f = Build();

        SendMessageResult result = await f.Svc.SendAsync(DefaultRequest(), CancellationToken.None);

        Assert.Equal(1, f.ConvRepo.AddCallCount);
        Assert.Equal(1, f.MsgRepo.AddCallCount);
        Assert.NotEqual(Guid.Empty, result.ConversationId);
        Assert.NotEqual(Guid.Empty, result.MessageId);
    }

    [Fact]
    public async Task SendAsync_WhenNoConversation_EnqueuesOutboxAndSavesOnce()
    {
        Fixtures f = Build();

        await f.Svc.SendAsync(DefaultRequest(), CancellationToken.None);

        Assert.Equal(1, f.Outbox.EnqueueCallCount);
        Assert.Equal("messaging.message-sent.v1", f.Outbox.LastEventType);
        Assert.Equal(1, f.Uow.SaveCallCount);
    }

    [Fact]
    public async Task SendAsync_WhenNoConversation_PayloadJsonContainsExpectedIds()
    {
        Fixtures f = Build();

        SendMessageResult result = await f.Svc.SendAsync(DefaultRequest(), CancellationToken.None);

        string payload = f.Outbox.LastPayloadJson!;
        Assert.Contains(result.ConversationId.ToString(), payload, StringComparison.Ordinal);
        Assert.Contains(result.MessageId.ToString(), payload, StringComparison.Ordinal);
        Assert.Contains(AMatchId.Value.ToString(), payload, StringComparison.Ordinal);
        Assert.Contains(SenderA.Value.ToString(), payload, StringComparison.Ordinal);
        Assert.Contains(SenderB.Value.ToString(), payload, StringComparison.Ordinal);
    }

    // ── Conversation already exists ──────────────────────────────────────────

    [Fact]
    public async Task SendAsync_WhenConversationExists_DoesNotAddConversationAgain()
    {
        Conversation existing = Conversation.CreateFromMatch(AMatchId, SenderA, SenderB, FixedNow);
        Fixtures f = Build(existingConversation: existing);

        await f.Svc.SendAsync(DefaultRequest(), CancellationToken.None);

        Assert.Equal(0, f.ConvRepo.AddCallCount);
    }

    [Fact]
    public async Task SendAsync_WhenConversationExists_AddsMessageEnqueuesAndSavesOnce()
    {
        Conversation existing = Conversation.CreateFromMatch(AMatchId, SenderA, SenderB, FixedNow);
        Fixtures f = Build(existingConversation: existing);

        SendMessageResult result = await f.Svc.SendAsync(DefaultRequest(), CancellationToken.None);

        Assert.Equal(1, f.MsgRepo.AddCallCount);
        Assert.Equal(1, f.Outbox.EnqueueCallCount);
        Assert.Equal("messaging.message-sent.v1", f.Outbox.LastEventType);
        Assert.Equal(1, f.Uow.SaveCallCount);
        Assert.NotEqual(Guid.Empty, result.MessageId);
    }

    // ── Sender not a participant ─────────────────────────────────────────────

    [Fact]
    public async Task SendAsync_WhenSenderNotInMatchParticipants_ThrowsBeforeConversationLookup()
    {
        // Sender is not one of the match's canonical participants —
        // the check should fire before any conversation or repo work.
        Fixtures f = Build();
        UserId outsider = new(Guid.NewGuid());
        SendMessageRequest req = new(AMatchId, outsider, "hi", null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => f.Svc.SendAsync(req, CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_WhenSenderNotInMatchParticipants_SkipsAllRepositoryOperations()
    {
        // Fail-fast: no conversation is created and nothing is persisted.
        Fixtures f = Build();
        UserId outsider = new(Guid.NewGuid());
        SendMessageRequest req = new(AMatchId, outsider, "hi", null);

        try
        {
            await f.Svc.SendAsync(req, CancellationToken.None);
        }
        catch (ArgumentException)
        {
            // expected
        }

        Assert.Equal(0, f.ConvRepo.AddCallCount);
        Assert.Equal(0, f.MsgRepo.AddCallCount);
        Assert.Equal(0, f.Outbox.EnqueueCallCount);
        Assert.Equal(0, f.Uow.SaveCallCount);
    }

    [Fact]
    public async Task SendAsync_WhenSenderIsNotParticipant_ThrowsArgumentException()
    {
        // Pre-seed a conversation so the outsider is provably not a participant.
        Conversation existing = Conversation.CreateFromMatch(AMatchId, SenderA, SenderB, FixedNow);
        Fixtures f = Build(existingConversation: existing);
        UserId outsider = new(Guid.NewGuid());
        SendMessageRequest req = new(AMatchId, outsider, "hi", null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => f.Svc.SendAsync(req, CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_WhenSenderIsNotParticipant_NoOutboxEnqueue()
    {
        // Outsider sends into an existing conversation where they are not a member.
        Conversation existing = Conversation.CreateFromMatch(AMatchId, SenderA, SenderB, FixedNow);
        Fixtures f = Build(existingConversation: existing);
        UserId outsider = new(Guid.NewGuid());
        SendMessageRequest req = new(AMatchId, outsider, "hi", null);

        try
        {
            await f.Svc.SendAsync(req, CancellationToken.None);
        }
        catch (ArgumentException)
        {
            // expected
        }

        Assert.Equal(0, f.Outbox.EnqueueCallCount);
        Assert.Equal(0, f.Uow.SaveCallCount);
    }

    // ── Text validation ──────────────────────────────────────────────────────

    [Fact]
    public async Task SendAsync_WhenTextIsWhitespaceOnly_ThrowsArgumentException()
    {
        Fixtures f = Build();

        await Assert.ThrowsAsync<ArgumentException>(
            () => f.Svc.SendAsync(DefaultRequest(text: "   "), CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_WhenTextExceeds2000Chars_ThrowsArgumentException()
    {
        Fixtures f = Build();
        string tooLong = new('x', 2001);

        await Assert.ThrowsAsync<ArgumentException>(
            () => f.Svc.SendAsync(DefaultRequest(text: tooLong), CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_WhenTextIsEmpty_ThrowsAndNoOutboxEnqueue()
    {
        Fixtures f = Build();

        try
        {
            await f.Svc.SendAsync(DefaultRequest(text: ""), CancellationToken.None);
        }
        catch (ArgumentException)
        {
            // expected
        }

        Assert.Equal(0, f.Outbox.EnqueueCallCount);
    }

    // ── Concurrent first-message race ───────────────────────────────────────

    [Fact]
    public async Task SendAsync_WhenConcurrentInsertRace_RetriesWithExistingConversation()
    {
        // Arrange: no conversation yet, but AddAsync will throw (simulating a unique-violation
        // translated by EfMessagingUnitOfWork), and the conflict conversation becomes visible
        // on the retry GetByMatchId.
        Conversation conflictConversation = Conversation.CreateFromMatch(AMatchId, SenderA, SenderB, FixedNow);

        Fixtures f = Build();
        f.ConvRepo.ThrowOnAdd = new ConversationAlreadyExistsException(AMatchId);
        f.ConvRepo.ConflictConversation = conflictConversation;

        SendMessageResult result = await f.Svc.SendAsync(DefaultRequest(), CancellationToken.None);

        // One AddAsync call (the failed attempt), no second AddAsync (retry uses existing).
        Assert.Equal(1, f.ConvRepo.AddCallCount);
        // Message was still added and outbox was enqueued.
        Assert.Equal(1, f.MsgRepo.AddCallCount);
        Assert.Equal(1, f.Outbox.EnqueueCallCount);
        Assert.Equal(1, f.Uow.SaveCallCount);
        Assert.NotEqual(Guid.Empty, result.MessageId);
    }

    [Fact]
    public async Task SendAsync_WhenConcurrentInsertRaceAndConversationStillMissing_Rethrows()
    {
        // Arrange: AddAsync throws but GetByMatchId still returns null (no winner yet).
        Fixtures f = Build();
        f.ConvRepo.ThrowOnAdd = new ConversationAlreadyExistsException(AMatchId);
        // ConflictConversation is null — store stays empty after the throw.

        await Assert.ThrowsAsync<ConversationAlreadyExistsException>(
            () => f.Svc.SendAsync(DefaultRequest(), CancellationToken.None));
    }

    // ── Correlation id is forwarded ──────────────────────────────────────────

    [Fact]
    public async Task SendAsync_ForwardsCorrelationIdToOutbox()
    {
        Fixtures f = Build();
        SendMessageRequest req = DefaultRequest() with { CorrelationId = "test-corr-id" };

        await f.Svc.SendAsync(req, CancellationToken.None);

        Assert.Equal("test-corr-id", f.Outbox.LastCorrelationId);
    }
}
