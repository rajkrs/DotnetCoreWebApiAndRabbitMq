using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace MessageQ
{
    public class RabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        MqConsumer _mqconsumer;
        public IConnection Connection { get; set; }
        public string ConsumerExchange { get; set; }
        public string ConnectionProviderName { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        bool _disposed;
        ILogger<RabbitMQPersistentConnection> _logger;

        public RabbitMQPersistentConnection(IServiceProvider serviceProvider, IConnectionFactory connectionFactory, ILogger<RabbitMQPersistentConnection> logger, string connectionProviderName)
        {
            ServiceProvider = serviceProvider;
            _logger = logger;
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            ConnectionProviderName = connectionProviderName;
            if (!IsConnected)
            {
                TryConnect();
            }
        }

        public void CreateConsumerChannel()
        {
            if (!IsConnected)
            {
                TryConnect();
            }


            var eventCollection = ServiceProvider.GetService(typeof(ConsumerCollection)) as ConsumerCollection;

            foreach (var @event in eventCollection.Collection)
            {
                var queue = ConsumerExchange +"_" + @event.Key.Name ;
                _mqconsumer = new MqConsumer(this, _logger, queue);
                _mqconsumer.CreateConsumerChannel();
            }


        }

        public void Disconnect()
        {
            if (_disposed)
            {
                return;
            }
            Dispose();
        }


        public bool IsConnected
        {
            get
            {
                return Connection != null && Connection.IsOpen && !_disposed;
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }
            return Connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                Connection.Dispose();
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public bool TryConnect()
        {
            var policy = Policy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
            {
                _logger.LogError(ex, "Unable to connect RabbitMq");
            });

            policy.Execute(() =>
            {
                Connection = _connectionFactory.CreateConnection(ConnectionProviderName);
                if (IsConnected)
                {
                    Connection.ConnectionShutdown += OnConnectionShutdown;
                    Connection.CallbackException += OnCallbackException;
                    Connection.ConnectionBlocked += OnConnectionBlocked;
                    _logger.LogWarning($"RabbitMQ persistent connection acquired a connection {Connection.Endpoint.HostName} and is subscribed to failure events");
                }
            });

            return true;
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;
            _logger.LogInformation("A RabbitMQ connection is shutdown. Trying to re-connect...");
            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;
            _logger.LogInformation("A RabbitMQ connection throw exception. Trying to re-connect...");
            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;
            _logger.LogInformation("A RabbitMQ connection is on shutdown. Trying to re-connect...");
            TryConnect();
        }
    }
}