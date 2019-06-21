using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQ
{
    public interface IRabbitMQPersistentConnection : IDisposable
    {
        bool IsConnected { get; }

        IConnection Connection { get; set; }

        string ConsumerExchange { get; set; }

        string ConnectionProviderName { get; set; }

        IServiceProvider ServiceProvider { get; set; }

        bool TryConnect();

        IModel CreateModel();

        void CreateConsumerChannel();

        void Disconnect();
    }
}
