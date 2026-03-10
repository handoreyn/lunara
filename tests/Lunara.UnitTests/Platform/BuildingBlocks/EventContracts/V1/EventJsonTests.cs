using System.Text.Json;
using Lunara.BuildingBlocks.EventContracts.V1;

namespace Lunara.UnitTests.Platform.BuildingBlocks.EventContracts.V1;

public sealed class EventJsonTests
{
    // ── Serialize ──────────────────────────────────────────────────────────

    [Fact]
    public void Serialize_WhenPayloadIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => EventJson.Serialize<SocialMatchCreatedV1>(null!));
    }

    [Fact]
    public void Serialize_PropertiesAreCamelCased()
    {
        SocialMatchCreatedV1 payload = new(
            MatchId: Guid.NewGuid(),
            User1Id: Guid.NewGuid(),
            User2Id: Guid.NewGuid(),
            OccurredAtUtc: DateTimeOffset.UtcNow);

        string json = EventJson.Serialize(payload);

        Assert.Contains("\"matchId\"", json, StringComparison.Ordinal);
        Assert.Contains("\"user1Id\"", json, StringComparison.Ordinal);
        Assert.Contains("\"user2Id\"", json, StringComparison.Ordinal);
        Assert.Contains("\"occurredAtUtc\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Serialize_NullPropertiesAreOmitted()
    {
        MessagingMessageSentV1 payload = new(
            ConversationId: Guid.NewGuid(),
            MessageId: Guid.NewGuid(),
            MatchId: Guid.NewGuid(),
            SenderId: Guid.NewGuid(),
            RecipientId: Guid.NewGuid(),
            Text: "hello",
            OccurredAtUtc: DateTimeOffset.UtcNow);

        string json = EventJson.Serialize(payload);

        // Verify no explicit null values are written
        Assert.DoesNotContain(":null", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Serialize_RoundTrip_PreservesAllFields()
    {
        Guid matchId = Guid.NewGuid();
        Guid user1Id = Guid.NewGuid();
        Guid user2Id = Guid.NewGuid();
        DateTimeOffset occurredAt = new(2026, 1, 15, 10, 30, 0, TimeSpan.Zero);

        SocialMatchCreatedV1 original = new(matchId, user1Id, user2Id, occurredAt);

        string json = EventJson.Serialize(original);
        SocialMatchCreatedV1 restored = EventJson.Deserialize<SocialMatchCreatedV1>(json);

        Assert.Equal(matchId, restored.MatchId);
        Assert.Equal(user1Id, restored.User1Id);
        Assert.Equal(user2Id, restored.User2Id);
        Assert.Equal(occurredAt, restored.OccurredAtUtc);
    }

    // ── Deserialize ────────────────────────────────────────────────────────

    [Fact]
    public void Deserialize_WhenJsonIsInvalid_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => EventJson.Deserialize<SocialMatchCreatedV1>("not-json"));
    }

    [Fact]
    public void Deserialize_WhenJsonIsLiteralNull_ThrowsJsonException()
    {
        Assert.Throws<JsonException>(() => EventJson.Deserialize<SocialMatchCreatedV1>("null"));
    }

    [Fact]
    public void Deserialize_IsCaseInsensitive_ForPropertyNames()
    {
        Guid matchId = Guid.NewGuid();
        Guid user1Id = Guid.NewGuid();
        Guid user2Id = Guid.NewGuid();

        // Use PascalCase property names (opposite of what Serialize emits)
        string json = $$"""
            {
                "MatchId": "{{matchId}}",
                "User1Id": "{{user1Id}}",
                "User2Id": "{{user2Id}}",
                "OccurredAtUtc": "2026-01-15T10:30:00+00:00"
            }
            """;

        SocialMatchCreatedV1 result = EventJson.Deserialize<SocialMatchCreatedV1>(json);

        Assert.Equal(matchId, result.MatchId);
        Assert.Equal(user1Id, result.User1Id);
        Assert.Equal(user2Id, result.User2Id);
    }

    [Fact]
    public void Deserialize_WhenJsonIsEmptyObject_ReturnsNonNullObject()
    {
        // An empty object maps all record properties to their defaults;
        // for a record with non-nullable Guid properties this produces
        // a valid object (Guid.Empty), not null — so no JsonException is thrown.
        SocialMatchCreatedV1 result = EventJson.Deserialize<SocialMatchCreatedV1>("{}");

        Assert.NotNull(result);
    }
}
