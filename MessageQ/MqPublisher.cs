using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageQ
{
    public class MqPublisher : IMqPublisher
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<MqPublisher> _looger;

        public MqPublisher(IRabbitMQPersistentConnection persistentConnection, ILogger<MqPublisher> _looger)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
        }


        public Task PublishAsync(Event @message, int? retryCount)
        {
            retryCount = retryCount ?? 3;//Default retry 3 times

            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }


            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(retryCount.Value, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _looger.LogWarning(ex, "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @message.Id, $"{time.TotalSeconds:n1}", ex.Message);
                });



            using (var channel = _persistentConnection.CreateModel())
            {

                var _exchange = _persistentConnection.Connection.ClientProvidedName.Split(',')[0];
                var _queue = _exchange +"_"+ @message.GetType().Name;
                var _routingKey = @message.GetType().Name;



                channel.ExchangeDeclare(exchange: _exchange, ExchangeType.Direct, true);


                channel.QueueDeclare(queue: _queue,
                                 durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

                channel.QueueBind(queue: _queue,
                                 exchange: _exchange,
                                 routingKey: _routingKey);



                string txtMessage = JsonConvert.SerializeObject(@message);
                var body = Encoding.UTF8.GetBytes(txtMessage);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent

                        properties.Headers = new Dictionary<string, object>();
                    properties.Headers.Add("typeof", @message.GetType().Name);//optional unique sender details in receiver side              



                        channel.ConfirmSelect();

                    channel.BasicPublish(exchange: _exchange,
                        routingKey: _routingKey,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);

                        channel.BasicAcks += (sender, eventArgs) =>
                        {
                            Debug.Write("Sent RabbitMQ");
                        };

                        channel.ConfirmSelect();

                });
            }

            return Task.CompletedTask;
        }





    }




}
