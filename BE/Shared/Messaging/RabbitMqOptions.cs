namespace Shared.Messaging
{
    public sealed class RabbitMqOptions
    {
        public string HostName { get; init; } = "localhost";
        public int Port { get; init; } = 5672;
        public string UserName { get; init; } = "admin";
        public string Password { get; init; } = "admin";
        public string VirtualHost { get; init; } = "/";
        public ushort PrefetchCount { get; init; } = 10;
        public string ServiceName { get; init; } = "unknown"; // for consumer naming
    }
}
