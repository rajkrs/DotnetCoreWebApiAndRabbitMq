using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orders.WebApi.Consumer.EventHandlers;
using Orders.WebApi.Consumer.Events;
using RabbitMQ.Client;

namespace Orders.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);


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
                .Add(typeof(OrderStausChangeEvent), typeof(OrderStausChangeEventHandler))
                );



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseMqConsumer("Shipping.WebApi");

            app.UseMvc();
        }
    }
}
