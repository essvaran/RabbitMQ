using Shared.Messaging;

namespace InventoryService
{
    public sealed class InventoryWorkerHost : BackgroundService
    {
        private readonly RabbitMqOptions _opts;
        private readonly int _workerCount;
        private readonly ushort _prefetch;
        private readonly List<IAsyncDisposable> _workers = new();

        public InventoryWorkerHost(RabbitMqOptions opts, int workerCount, ushort prefetch)
        {
            _opts = opts;
            _workerCount = workerCount;
            _prefetch = prefetch;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Spin up competing consumers
            for (int i = 0; i < _workerCount; i++)
            {
                var workerCreated = new OrdersCreatedConsumer(_opts, _prefetch);
                var workerCancelled = new OrdersCancelledConsumer(_opts, _prefetch);
                workerCreated.Start();
                workerCancelled.Start();
                _workers.Add(workerCreated);
                _workers.Add(workerCancelled);
            }

            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var w in _workers)
            {
                await w.DisposeAsync();
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
