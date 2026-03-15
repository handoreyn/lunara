namespace Lunara.Messaging.Domain.ValueObjects;

/// <summary>Strongly-typed identifier for a <see cref="Entities.Conversation"/>.</summary>
public readonly record struct ConversationId
{
    /// <summary>The underlying <see cref="Guid"/> value.</summary>
    public Guid Value { get; }

    /// <summary>Initializes a new <see cref="ConversationId"/>.</summary>
    /// <param name="value">The underlying identifier. Must not be <see cref="Guid.Empty"/>.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public ConversationId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ConversationId must not be empty.", nameof(value));
        }

        Value = value;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
