using Lunara.Api.Messaging;
using Lunara.Api.Moderation;
using Lunara.Api.Social;
using Lunara.Infrastructure.Host;
using Lunara.Messaging.Application.DTOs;
using Lunara.Messaging.Application.Exceptions;
using Lunara.Messaging.Application.UseCases;
using Lunara.Moderation.Application.DTOs;
using Lunara.Moderation.Application.UseCases;
using Lunara.Social.Application.DTOs;
using Lunara.Social.Application.UseCases;
using Lunara.Social.Domain;
using Lunara.Social.Domain.ValueObjects;
using MessagingMatchId = Lunara.Messaging.Domain.ValueObjects.MatchId;
using MessagingUserId = Lunara.Messaging.Domain.ValueObjects.UserId;
using ModerationUserId = Lunara.Moderation.Domain.ValueObjects.UserId;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddLunaraPlatform(builder.Configuration);
builder.Services.AddLunaraModules();

WebApplication app = builder.Build();

app.MapGet("/health", () => Results.Ok());
app.MapGet("/ready", async (IDatabaseReadinessChecker checker, CancellationToken ct) =>
{
    bool isReady = await checker.IsReadyAsync(ct).ConfigureAwait(false);
    return isReady ? Results.Ok() : Results.StatusCode(503);
});

app.MapPost("/v1/social/swipe", async (
    HttpContext http,
    SwipeBody body,
    RecordSwipeService svc,
    CancellationToken ct) =>
{
    string? actorIdHeader = http.Request.Headers["X-User-Id"];

    if (string.IsNullOrEmpty(actorIdHeader)
        || !Guid.TryParse(actorIdHeader, out Guid actorGuid)
        || actorGuid == Guid.Empty)
    {
        return Results.BadRequest("Missing or invalid X-User-Id header.");
    }

    if (body.TargetUserId == Guid.Empty)
    {
        return Results.BadRequest("targetUserId must not be empty.");
    }

    if (!Enum.TryParse<SwipeActionType>(body.Action, ignoreCase: true, out SwipeActionType action))
    {
        return Results.BadRequest("Invalid action. Must be 'like' or 'pass'.");
    }

    string? correlationId = http.Request.Headers["X-Correlation-Id"];
    if (string.IsNullOrEmpty(correlationId))
    {
        correlationId = null;
    }

    RecordSwipeRequest request = new(new UserId(actorGuid), new UserId(body.TargetUserId), action)
    {
        CorrelationId = correlationId,
    };
    RecordSwipeResult result = await svc.RecordAsync(request, ct).ConfigureAwait(false);
    return Results.Ok(result);
});

app.MapPost("/v1/messaging/send", async (
    HttpContext http,
    SendMessageBody body,
    SendMessageService svc,
    CancellationToken ct) =>
{
    string? senderIdHeader = http.Request.Headers["X-User-Id"];

    if (string.IsNullOrEmpty(senderIdHeader)
        || !Guid.TryParse(senderIdHeader, out Guid senderGuid)
        || senderGuid == Guid.Empty)
    {
        return Results.BadRequest("Missing or invalid X-User-Id header.");
    }

    if (body.MatchId == Guid.Empty)
    {
        return Results.BadRequest("matchId must not be empty.");
    }

    if (string.IsNullOrWhiteSpace(body.Text))
    {
        return Results.BadRequest("text must not be empty.");
    }

    if (body.Text.Trim().Length > 2000)
    {
        return Results.BadRequest("text must not exceed 2000 characters.");
    }

    string? correlationId = http.Request.Headers["X-Correlation-Id"];
    if (string.IsNullOrEmpty(correlationId))
    {
        correlationId = null;
    }

    SendMessageRequest request = new(
        new MessagingMatchId(body.MatchId),
        new MessagingUserId(senderGuid),
        body.Text,
        correlationId);

    try
    {
        SendMessageResult result = await svc.SendAsync(request, ct).ConfigureAwait(false);
        return Results.Ok(result);
    }
    catch (MatchNotFoundException)
    {
        return Results.BadRequest("Match not found.");
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/v1/moderation/block", async (
    HttpContext http,
    BlockBody body,
    BlockUserService svc,
    CancellationToken ct) =>
{
    string? blockerIdHeader = http.Request.Headers["X-User-Id"];

    if (string.IsNullOrEmpty(blockerIdHeader)
        || !Guid.TryParse(blockerIdHeader, out Guid blockerGuid)
        || blockerGuid == Guid.Empty)
    {
        return Results.BadRequest("Missing or invalid X-User-Id header.");
    }

    if (body.BlockedUserId == Guid.Empty)
    {
        return Results.BadRequest("blockedUserId must not be empty.");
    }

    if (blockerGuid == body.BlockedUserId)
    {
        return Results.BadRequest("A user cannot block themselves.");
    }

    BlockUserRequest request = new(
        new ModerationUserId(blockerGuid),
        new ModerationUserId(body.BlockedUserId));

    BlockUserResult result = await svc.BlockAsync(request, ct).ConfigureAwait(false);
    return Results.Ok(result);
});

app.MapPost("/v1/moderation/report", async (
    HttpContext http,
    ReportBody body,
    ReportUserService svc,
    CancellationToken ct) =>
{
    string? reporterIdHeader = http.Request.Headers["X-User-Id"];

    if (string.IsNullOrEmpty(reporterIdHeader)
        || !Guid.TryParse(reporterIdHeader, out Guid reporterGuid)
        || reporterGuid == Guid.Empty)
    {
        return Results.BadRequest("Missing or invalid X-User-Id header.");
    }

    if (body.TargetUserId == Guid.Empty)
    {
        return Results.BadRequest("targetUserId must not be empty.");
    }

    if (reporterGuid == body.TargetUserId)
    {
        return Results.BadRequest("A user cannot report themselves.");
    }

    if (string.IsNullOrWhiteSpace(body.Reason))
    {
        return Results.BadRequest("reason must not be empty.");
    }

    ReportUserRequest request = new(
        new ModerationUserId(reporterGuid),
        new ModerationUserId(body.TargetUserId),
        body.Reason,
        body.Details);

    ReportUserResult result = await svc.ReportAsync(request, ct).ConfigureAwait(false);
    return Results.Ok(result);
});

await app.RunAsync().ConfigureAwait(false);
