using MessageQ;
using Shipping.WebApi.Consumer.Events;
using Shipping.WebApi.Publish.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shipping.WebApi.Consumer.EventHandlers
{
    public class ShippingRequestHandler : IEventHandler<ShippingReqEvent>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IMqPublisher mqPublisher;

        
        public ShippingRequestHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            mqPublisher = _serviceProvider.GetService(typeof(IMqPublisher)) as IMqPublisher;
        }

        public Task Handle(ShippingReqEvent @event)
        {
            mqPublisher.PublishAsync( new OrderStausChangeEvent { OrderId = @event.OrderId, Status = "Dispatch" });

            return Task.CompletedTask;
        }
    }
}
