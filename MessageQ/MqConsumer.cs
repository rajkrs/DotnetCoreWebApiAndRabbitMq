using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MessageQ
{
    public class MqConsumer 
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private IModel _consumerChannel;
        private readonly ILogger<Object> _logger;
        private string _queue;
        public MqConsumer(IRabbitMQPersistentConnection persistentConnection, ILogger<Object>  logger, string queue = null)
        {
            _persistentConnection = persistentConnection;
            _queue = queue;
            _logger = logger;
        }

        public IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var channel = _persistentConnection.CreateModel();
            channel.QueueDeclare(queue: _queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += Consume;

            channel.BasicConsume(queue: _queue, autoAck: true, consumer: consumer);
            channel.CallbackException += (sender, ea) =>
            {
                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();
            };
            return channel;
        }


        private void Consume(object sender, BasicDeliverEventArgs e)
        {
            var routingKey = e.RoutingKey;
            var message = Encoding.UTF8.GetString(e.Body);

            var eventCollection = _persistentConnection.ServiceProvider.GetService(typeof(ConsumerCollection)) as ConsumerCollection;

            var @eventType = eventCollection.Collection.Where(d => d.Key.Name == routingKey).FirstOrDefault().Key;
            var eventHandlerType = eventCollection.Collection[@eventType];

            ConstructorInfo conEventHandlerType = eventHandlerType.GetConstructor(new[] { typeof(IServiceProvider) });
            object magicClassObject = conEventHandlerType.Invoke(new object[] { _persistentConnection.ServiceProvider });

            

            MethodInfo handleMethod = eventHandlerType.GetMethod("Handle");
            object magicValue = handleMethod.Invoke(magicClassObject, new object[] { JsonConvert.DeserializeObject(message, @eventType) });
            
        }





    }




}
