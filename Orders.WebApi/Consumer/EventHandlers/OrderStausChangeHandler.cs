using MessageQ;
using Orders.WebApi.Consumer.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.WebApi.Consumer.EventHandlers
{
    
    public class OrderStausChangeEventHandler : IEventHandler<OrderStausChangeEvent>
    {
        private readonly IServiceProvider _serviceProvider;
        public OrderStausChangeEventHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task Handle(OrderStausChangeEvent @event)
        {
            return Task.CompletedTask;
        }
    }
}
