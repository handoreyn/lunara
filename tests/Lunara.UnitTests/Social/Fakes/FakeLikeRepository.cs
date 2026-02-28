using Lunara.Social.Application.Ports;
using Lunara.Social.Domain.ValueObjects;

namespace Lunara.UnitTests.Social.Fakes;

/// <summary>In-memory fake for <see cref="ILikeRepository"/> used in unit tests.</summary>
public sealed class FakeLikeRepository : ILikeRepository
{
    private readonly HashSet<(UserId From, UserId To)> _likes = [];

    /// <summary>Gets the number of times <see cref="AddLikeAsync"/> was called.</summary>
    public int AddLikeCallCount { get; private set; }

    /// <summary>Seeds an existing like so reciprocal-like logic can be exercised.</summary>
    public void SeedLike(UserId from, UserId to)
    {
        _likes.Add((from, to));
    }

    /// <inheritdoc/>
    public Task<bool> ExistsLikeAsync(UserId from, UserId recipient, CancellationToken ct)
    {
        return Task.FromResult(_likes.Contains((from, recipient)));
    }

    /// <inheritdoc/>
    public Task AddLikeAsync(UserId from, UserId recipient, DateTimeOffset atUtc, CancellationToken ct)
    {
        _likes.Add((from, recipient));
        AddLikeCallCount++;
        return Task.CompletedTask;
    }
}
