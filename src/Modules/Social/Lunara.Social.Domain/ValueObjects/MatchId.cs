namespace Lunara.Social.Domain.ValueObjects;

/// <summary>Strongly-typed identifier for a match between two users.</summary>
public readonly record struct MatchId
{
    /// <summary>The underlying <see cref="Guid"/> value.</summary>
    public Guid Value { get; }

    /// <summary>Initializes a new <see cref="MatchId"/>.</summary>
    /// <param name="value">The underlying identifier. Must not be <see cref="Guid.Empty"/>.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public MatchId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("MatchId must not be empty.", nameof(value));
        }

        Value = value;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
