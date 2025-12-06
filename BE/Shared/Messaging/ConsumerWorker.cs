using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Shared.Messaging
{
    public abstract class ConsumerWorker<T> : IAsyncDisposable
    {
        private readonly IConnection _connection;
        protected readonly IChannel _channel;
        private readonly string _queue;
        private readonly ushort _prefetch;

        protected ConsumerWorker(RabbitMqOptions options, string queue, ushort prefetch)
        {
            _queue = queue;
            _prefetch = prefetch;

            _connection = RabbitMqConnectionFactory.Create(options);
            _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
            RabbitMqTopology.Declare(_channel);
            _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _prefetch, global: false).GetAwaiter().GetResult();
        }

        public void Start()
        {
            var consumer = new AsyncBasicConsumer(_channel, async (body, props, deliveryTag) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(json)!;
                    await Handle(message, props.Headers);
                    await _channel.BasicAckAsync(deliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Consumer error: {ex.Message}");
                    await _channel.BasicNackAsync(deliveryTag, multiple: false, requeue: false);
                }
            });

            _channel.BasicConsumeAsync(
                queue: _queue,
                autoAck: false,
                consumerTag: string.Empty,
                noLocal: false,
                exclusive: false,
                arguments: null,
                consumer: consumer,
                cancellationToken: default
            ).GetAwaiter().GetResult();
        }

        protected abstract Task Handle(T message, IDictionary<string, object>? headers);

        public async ValueTask DisposeAsync()
        {
            await _channel.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
