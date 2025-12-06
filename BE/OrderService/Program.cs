using Shared.Contracts;
using Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Bind RabbitMQ options from configuration
var mqOpts = builder.Configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions { ServiceName = "order-service" };
builder.Services.AddSingleton(mqOpts);
builder.Services.AddSingleton<Publisher>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

// POST /orders
app.MapPost("/orders", (Publisher publisher, OrderCreateRequest req, RabbitMqOptions opts) =>
{
    var evt = new OrderCreatedEvent(Guid.NewGuid().ToString(), req.UserId, req.Amount, DateTimeOffset.UtcNow);
    var headers = new Dictionary<string, object>
    {
        { "correlation-id", Guid.NewGuid().ToString() },
        { "producer", opts.ServiceName }
    };

    publisher.PublishDirect(RoutingKeys.OrderCreated, evt, headers);
    publisher.PublishFanout(new { type = "notification", message = $"Order created for {req.UserId}" }, headers);

    return Results.Created($"/orders/{evt.OrderId}", new { evt.OrderId });
});

app.MapPost("/orders/{id}/cancel", (string id, Publisher publisher, CancelRequest req, RabbitMqOptions opts) =>
{
    var evt = new OrderCancelledEvent(id, req.Reason, DateTimeOffset.UtcNow);
    var headers = new Dictionary<string, object>
    {
        { "correlation-id", Guid.NewGuid().ToString() },
        { "producer", opts.ServiceName }
    };

    publisher.PublishDirect(RoutingKeys.OrderCancelled, evt, headers);
    publisher.PublishFanout(new { type = "notification", message = $"Order {id} cancelled" }, headers);

    return Results.Ok(new { id, status = "cancelled" });
});

app.Run();

record OrderCreateRequest(string UserId, decimal Amount);
record CancelRequest(string Reason);
