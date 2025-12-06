using RabbitMQ.Client;
using System.Threading.Tasks;

namespace Shared.Messaging
{
    public sealed class RabbitMqConnectionFactory
    {
        public static IConnection Create(RabbitMqOptions opts)
        {
            var factory = new ConnectionFactory
            {
                HostName = opts.HostName,
                Port = opts.Port,
                UserName = opts.UserName,
                Password = opts.Password,
                VirtualHost = opts.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };
            // Use async connection creation and wait synchronously
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        }
    }
}
