using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Shared.Messaging
{
    public class AsyncBasicConsumer : IAsyncBasicConsumer
    {
        private readonly Func<byte[], IReadOnlyBasicProperties, ulong, Task> _onMessage;
        public IChannel Channel { get; set; }

        public AsyncBasicConsumer(IChannel channel, Func<byte[], IReadOnlyBasicProperties, ulong, Task> onMessage)
        {
            Channel = channel;
            _onMessage = onMessage;
        }

        public Task HandleBasicCancelAsync(string consumerTag, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task HandleBasicCancelOkAsync(string consumerTag, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task HandleBasicConsumeOkAsync(string consumerTag, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task HandleChannelShutdownAsync(object channel, ShutdownEventArgs reason) => Task.CompletedTask;
        public async Task HandleBasicDeliverAsync(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IReadOnlyBasicProperties properties, ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
        {
            await _onMessage(body.ToArray(), properties, deliveryTag);
        }
    }
}
