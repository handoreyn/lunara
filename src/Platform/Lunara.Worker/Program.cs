using Lunara.Worker;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<LunaraWorker>();

IHost host = builder.Build();
await host.RunAsync();
