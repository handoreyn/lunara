namespace Lunara.Moderation.Domain.ValueObjects;

/// <summary>Strongly-typed identifier for a block between two users.</summary>
public readonly record struct BlockId
{
    /// <summary>The underlying <see cref="Guid"/> value.</summary>
    public Guid Value { get; }

    /// <summary>Initializes a new <see cref="BlockId"/>.</summary>
    /// <param name="value">The underlying identifier. Must not be <see cref="Guid.Empty"/>.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public BlockId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("BlockId must not be empty.", nameof(value));
        }

        Value = value;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
