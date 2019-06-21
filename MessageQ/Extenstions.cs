using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageQ
{
    public static class Extenstions
    {
        public static IRabbitMQPersistentConnection ListenerConnection;
        public static IApplicationBuilder UseMqConsumer(this IApplicationBuilder app, string consumerExchange)
        {

            var serviceProvider = app.ApplicationServices.GetService(typeof(IServiceProvider)) as IServiceProvider;
            ListenerConnection = app.ApplicationServices.GetService(typeof(IRabbitMQPersistentConnection)) as IRabbitMQPersistentConnection;
            ListenerConnection.ConsumerExchange = consumerExchange;

            var life = app.ApplicationServices.GetService(typeof(IApplicationLifetime)) as IApplicationLifetime;
            life.ApplicationStarted.Register(() => ListenerConnection.CreateConsumerChannel());
            life.ApplicationStopping.Register(() => ListenerConnection.Disconnect());
            return app;
        }


    }
}
