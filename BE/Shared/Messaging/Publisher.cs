using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging
{
    public sealed class Publisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public Publisher(RabbitMqOptions options)
        {
            _connection = RabbitMqConnectionFactory.Create(options);
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            RabbitMqTopology.Declare(_channel);
        }

        public void PublishDirect<T>(string routingKey, T message, IDictionary<string, object>? headers = null)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var props = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Headers = headers ?? new Dictionary<string, object>()
            };
            _channel.BasicPublishAsync(RabbitMqTopology.ExchangeDirect,
                                  routingKey,
                                  true,
                                  props,
                                  body).GetAwaiter().GetResult();
        }

        public void PublishFanout<T>(T message, IDictionary<string, object>? headers = null)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var props = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Headers = headers ?? new Dictionary<string, object>()
            };
            _channel.BasicPublishAsync(RabbitMqTopology.ExchangeFanout,
                                  "",
                                  false,
                                  props,
                                  body).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _channel?.DisposeAsync().GetAwaiter().GetResult();
            _connection?.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}
