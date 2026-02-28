using Lunara.Infrastructure.Host;
using Lunara.Worker;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLunaraModules();
builder.Services.AddHostedService<LunaraWorker>();

IHost host = builder.Build();
await host.RunAsync();
