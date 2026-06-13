using Habeas.Application;
using Habeas.Bot;
using Habeas.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

// Wire the layers from the outside in. Each layer owns its own registrations.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddTelegramBot(builder.Configuration);

var host = builder.Build();
host.Run();
