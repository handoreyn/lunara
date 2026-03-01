using Lunara.Infrastructure.Host;
using Lunara.Worker;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLunaraPlatform(builder.Configuration);
builder.Services.AddLunaraModules();
builder.Services.AddHostedService<LunaraWorker>();
builder.Services.AddHostedService<OutboxPublisherHostedService>();

IHost host = builder.Build();
await host.RunAsync().ConfigureAwait(false);
