namespace Lunara.Notifications.Domain;

/// <summary>
/// Discriminates the kind of event that triggered a notification.
/// </summary>
public enum NotificationType
{
    /// <summary>Default unset value; should not appear on a persisted notification.</summary>
    None = 0,

    /// <summary>A mutual match between two users was created.</summary>
    MatchCreated = 1,

    /// <summary>A new message was received in a conversation.</summary>
    MessageReceived = 2,
}
