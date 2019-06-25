using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        public static IServiceCollection AddMqService(this IServiceCollection services,  string mqConnectionName)
        {
            services.AddSingleton(s => s.GetRequiredService<IOptionsMonitor<RabbitMQConfigurations>>().CurrentValue);

            services.AddSingleton<IRabbitMQPersistentConnection>(s =>
            {
                var configurations = s.GetRequiredService<IOptionsMonitor<RabbitMQConfigurations>>().CurrentValue;
                var factory = new ConnectionFactory()
                {
                    HostName = configurations.HostName,
                    Port = configurations.Port,
                    UserName = configurations.UserName,
                    Password = configurations.Password
                };
                var logger = s.GetRequiredService<ILogger<RabbitMQPersistentConnection>>();
                var serviceProvider = s.GetRequiredService<IServiceProvider>();
                return new RabbitMQPersistentConnection(serviceProvider, factory, logger, mqConnectionName);

            });


            return services;
        }



    }
}
