using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Messaging;
using InventoryService;

var builder = Host.CreateApplicationBuilder(args);

var mqOpts = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions { ServiceName = "inventory-service" };
builder.Services.AddSingleton(mqOpts);
builder.Services.AddHostedService(sp => new InventoryWorkerHost(sp.GetRequiredService<RabbitMqOptions>(), workerCount: 4, prefetch: mqOpts.PrefetchCount));


var app = builder.Build();

app.Run();
