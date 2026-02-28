WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok());

await app.RunAsync();
