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

            //register option monitor from json config
            services.Configure<RabbitMQConfigurations>(Configuration.GetSection("RabbitMQ"));
            //Add custom rabbitmq service 
            services.AddMqService(GetType().Assembly.FullName);

            //Add publisher
            services.AddSingleton<IMqPublisher, MqPublisher>();


            //Add event and respective handler for consumer.
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
