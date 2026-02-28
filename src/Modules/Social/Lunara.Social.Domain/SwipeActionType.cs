namespace Lunara.Social.Domain;

/// <summary>Represents the action a user takes when viewing another user's profile.</summary>
public enum SwipeActionType
{
    /// <summary>The user has liked the target profile.</summary>
    Like,

    /// <summary>The user has passed on the target profile.</summary>
    Pass,
}
