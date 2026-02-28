using Lunara.Api.Social;
using Lunara.Infrastructure.Host;
using Lunara.Social.Application.DTOs;
using Lunara.Social.Application.UseCases;
using Lunara.Social.Domain;
using Lunara.Social.Domain.ValueObjects;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddLunaraModules();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapGet("/health", () => Results.Ok());

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

    RecordSwipeRequest request = new(new UserId(actorGuid), new UserId(body.TargetUserId), action);
    RecordSwipeResult result = await svc.RecordAsync(request, ct);
    return Results.Ok(result);
});

await app.RunAsync();
