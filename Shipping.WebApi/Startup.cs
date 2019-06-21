using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shipping.WebApi.Consumer.EventHandlers;
using Shipping.WebApi.Consumer.Events;

namespace Shipping.WebApi
{
    public class Startup
    {

        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RabbitMQConfigurations>(Configuration.GetSection("RabbitMQ"));
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
                return new RabbitMQPersistentConnection(serviceProvider, factory, logger, GetType().Assembly.FullName);
            });

            services.AddSingleton<IMqPublisher, MqPublisher>();

            services.AddSingleton<ConsumerCollection>(new ConsumerCollection()
                .Add(typeof(ShippingReqEvent), typeof(ShippingRequestHandler))
                );

            


            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMqConsumer("Orders.WebApi");
            


            app.UseMvc();

           

        }



    }
}
