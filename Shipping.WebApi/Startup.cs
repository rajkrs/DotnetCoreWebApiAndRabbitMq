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
            //register option monitor from json config
            services.Configure<RabbitMQConfigurations>(Configuration.GetSection("RabbitMQ"));
            //Add custom rabbitmq service 
            services.AddMqService(GetType().Assembly.FullName);

            //Add publisher
            services.AddSingleton<IMqPublisher, MqPublisher>();

            //Add event and respective handler for consumer.
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
