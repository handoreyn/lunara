namespace Lunara.Moderation.Domain.ValueObjects;

/// <summary>Strongly-typed identifier for a report submitted about a user.</summary>
public readonly record struct ReportId
{
    /// <summary>The underlying <see cref="Guid"/> value.</summary>
    public Guid Value { get; }

    /// <summary>Initializes a new <see cref="ReportId"/>.</summary>
    /// <param name="value">The underlying identifier. Must not be <see cref="Guid.Empty"/>.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is <see cref="Guid.Empty"/>.</exception>
    public ReportId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ReportId must not be empty.", nameof(value));
        }

        Value = value;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
