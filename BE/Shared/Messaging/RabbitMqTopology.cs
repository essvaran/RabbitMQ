using RabbitMQ.Client;

namespace Shared.Messaging
{
    public static class RabbitMqTopology
    {
        public const string ExchangeDirect = "events.direct";
        public const string ExchangeFanout = "events.fanout";

        public static void Declare(IChannel channel)
        {
            channel.ExchangeDeclareAsync(ExchangeDirect, ExchangeType.Direct, durable: true, autoDelete: false).GetAwaiter().GetResult();
            channel.ExchangeDeclareAsync(ExchangeFanout, ExchangeType.Fanout, durable: true, autoDelete: false).GetAwaiter().GetResult();
            DeclareQueueWithDlq(channel, "orders.inventory", deadLetterExchange: "dlx.direct");
            channel.ExchangeDeclareAsync("dlx.direct", ExchangeType.Direct, durable: true).GetAwaiter().GetResult();
            channel.QueueBindAsync("orders.inventory", ExchangeDirect, "order.created").GetAwaiter().GetResult();
            channel.QueueBindAsync("orders.inventory", ExchangeDirect, "order.cancelled").GetAwaiter().GetResult();
            DeclareQueueWithDlq(channel, "notifications.email", deadLetterExchange: "dlx.fanout");
            channel.ExchangeDeclareAsync("dlx.fanout", ExchangeType.Direct, durable: true).GetAwaiter().GetResult();
            channel.QueueBindAsync("notifications.email", ExchangeFanout, "").GetAwaiter().GetResult();
        }

        private static void DeclareQueueWithDlq(IChannel channel, string queueName, string deadLetterExchange)
        {
            var dlq = $"{queueName}.dlq";
            channel.ExchangeDeclareAsync(deadLetterExchange, ExchangeType.Direct, durable: true).GetAwaiter().GetResult();
            channel.QueueDeclareAsync(dlq, durable: true, exclusive: false, autoDelete: false).GetAwaiter().GetResult();
            channel.QueueBindAsync(dlq, deadLetterExchange, routingKey: queueName).GetAwaiter().GetResult();
            var args = new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", deadLetterExchange },
                { "x-dead-letter-routing-key", queueName },
                { "x-queue-mode", "lazy" }
            };
            channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: args).GetAwaiter().GetResult();
        }
    }
}
