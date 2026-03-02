using Lunara.Messaging.Domain.DomainEvents;
using Lunara.Messaging.Domain.Entities;
using Lunara.Messaging.Domain.ValueObjects;

namespace Lunara.UnitTests.Modules.Messaging.Domain;

public sealed class ConversationTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    private static readonly UserId SmallUser = new(new Guid("00000000-0000-0000-0000-000000000001"));
    private static readonly UserId LargeUser = new(new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"));
    private static readonly MatchId AMatchId = new(Guid.NewGuid());
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static Conversation MakeConversation(UserId u1, UserId u2)
    {
        return Conversation.CreateFromMatch(AMatchId, u1, u2, Now);
    }

    // ── CreateFromMatch ────────────────────────────────────────────────────

    [Fact]
    public void CreateFromMatch_WhenUser1IsLarger_StoresSmallGuidAsUser1Id()
    {
        // user1 arg is the larger Guid — canonical ordering must swap them
        Conversation conv = MakeConversation(LargeUser, SmallUser);

        Assert.Equal(SmallUser, conv.User1Id);
        Assert.Equal(LargeUser, conv.User2Id);
    }

    [Fact]
    public void CreateFromMatch_WhenUser1IsSmaller_KeepsOriginalOrder()
    {
        Conversation conv = MakeConversation(SmallUser, LargeUser);

        Assert.Equal(SmallUser, conv.User1Id);
        Assert.Equal(LargeUser, conv.User2Id);
    }

    [Fact]
    public void CreateFromMatch_SetsMatchIdAndCreatedAtUtc()
    {
        Conversation conv = MakeConversation(SmallUser, LargeUser);

        Assert.Equal(AMatchId, conv.MatchId);
        Assert.Equal(Now, conv.CreatedAtUtc);
    }

    [Fact]
    public void CreateFromMatch_WithSameUser_Throws()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => MakeConversation(SmallUser, SmallUser));

        Assert.Contains("two distinct users", ex.Message, StringComparison.Ordinal);
    }

    // ── SendMessage – guard clauses ────────────────────────────────────────

    [Fact]
    public void SendMessage_WhenSenderIsNotParticipant_Throws()
    {
        Conversation conv = MakeConversation(SmallUser, LargeUser);
        UserId outsider = new(Guid.NewGuid());

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => conv.SendMessage(outsider, "hi", Now));

        Assert.Contains("not a participant", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SendMessage_WhenTextIsEmpty_Throws()
    {
        Conversation conv = MakeConversation(SmallUser, LargeUser);

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => conv.SendMessage(SmallUser, "   ", Now));

        Assert.Contains("must not be empty", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SendMessage_WhenTextExceeds2000Chars_Throws()
    {
        Conversation conv = MakeConversation(SmallUser, LargeUser);
        string tooLong = new('a', 2001);

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => conv.SendMessage(SmallUser, tooLong, Now));

        Assert.Contains("must not exceed 2000 characters", ex.Message, StringComparison.Ordinal);
    }

    // ── SendMessage – text trimming ────────────────────────────────────────

    [Fact]
    public void SendMessage_TrimsWhitespaceFromText()
    {
        Conversation conv = MakeConversation(SmallUser, LargeUser);

        (Message message, MessageSent _) = conv.SendMessage(SmallUser, "  hello  ", Now);

        Assert.Equal("hello", message.Text);
    }

    [Fact]
    public void SendMessage_AcceptsTextOfExactly2000Chars()
    {
        Conversation conv = MakeConversation(SmallUser, LargeUser);
        string exact = new('a', 2000);

        (Message message, MessageSent _) = conv.SendMessage(SmallUser, exact, Now);

        Assert.Equal(exact, message.Text);
    }

    // ── SendMessage – recipient assignment ────────────────────────────────

    [Fact]
    public void SendMessage_WhenUser1Sends_RecipientIsUser2()
    {
        Conversation conv = MakeConversation(SmallUser, LargeUser);

        (Message message, MessageSent _) = conv.SendMessage(SmallUser, "hey", Now);

        Assert.Equal(SmallUser, message.SenderId);
        Assert.Equal(LargeUser, message.RecipientId);
    }

    [Fact]
    public void SendMessage_WhenUser2Sends_RecipientIsUser1()
    {
        Conversation conv = MakeConversation(SmallUser, LargeUser);

        (Message message, MessageSent _) = conv.SendMessage(LargeUser, "hey", Now);

        Assert.Equal(LargeUser, message.SenderId);
        Assert.Equal(SmallUser, message.RecipientId);
    }

    // ── SendMessage – domain event ─────────────────────────────────────────

    [Fact]
    public void SendMessage_EventFields_MatchMessageAndConversation()
    {
        Conversation conv = MakeConversation(SmallUser, LargeUser);
        DateTimeOffset sentAt = new(2026, 3, 2, 12, 0, 0, TimeSpan.Zero);

        (Message message, MessageSent evt) = conv.SendMessage(SmallUser, "  hello world  ", sentAt);

        Assert.NotEqual(Guid.Empty, evt.EventId);
        Assert.Equal(conv.Id, evt.ConversationId);
        Assert.Equal(message.Id, evt.MessageId);
        Assert.Equal(SmallUser, evt.SenderId);
        Assert.Equal(LargeUser, evt.RecipientId);
        Assert.Equal("hello world", evt.Text);
        Assert.Equal(sentAt, evt.OccurredAtUtc);
    }

    [Fact]
    public void SendMessage_AddsMessageToConversationList()
    {
        Conversation conv = MakeConversation(SmallUser, LargeUser);

        (Message message, MessageSent _) = conv.SendMessage(SmallUser, "first", Now);

        Assert.Single(conv.Messages);
        Assert.Equal(message.Id, conv.Messages[0].Id);
    }
}
