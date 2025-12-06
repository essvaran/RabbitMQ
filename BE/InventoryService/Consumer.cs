using Shared.Contracts;
using Shared.Messaging;

namespace InventoryService
{
    public sealed class OrdersCreatedConsumer : ConsumerWorker<OrderCreatedEvent>
    {
        public OrdersCreatedConsumer(RabbitMqOptions opts, ushort prefetch)
            : base(opts, queue: "orders.inventory", prefetch: prefetch) { }

        protected override async Task Handle(OrderCreatedEvent message, IDictionary<string, object>? headers)
        {
            // Idempotency: Use message.OrderId as natural key; check if already processed
            Console.WriteLine($"[Inventory] Reserve stock for Order {message.OrderId} Amount {message.Amount}");
            await Task.Delay(100); // simulate work
        }
    }

    public sealed class OrdersCancelledConsumer : ConsumerWorker<OrderCancelledEvent>
    {
        public OrdersCancelledConsumer(RabbitMqOptions opts, ushort prefetch)
            : base(opts, queue: "orders.inventory", prefetch: prefetch) { }

        protected override async Task Handle(OrderCancelledEvent message, IDictionary<string, object>? headers)
        {
            Console.WriteLine($"[Inventory] Release stock for Order {message.OrderId}, Reason {message.Reason}");
            await Task.Delay(100); // simulate work
        }
    }
}
